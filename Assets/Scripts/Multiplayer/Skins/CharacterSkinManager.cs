using UnityEngine;
using Unity.Netcode;
using System.Collections;
using System.Collections.Generic;

namespace SlapArena.Multiplayer
{
    /// <summary>
    /// Gestiona el cambio de skins de forma profesional y escalable.
    /// Unifica jerarquías de huesos automáticamente para animaciones Generic.
    /// </summary>
    public class CharacterSkinManager : NetworkBehaviour
    {
        [Header("Configuración de Skins")]
        public List<CharacterSkin> availableSkins;
        public Transform skinContainer; // Donde se instanciará el modelo

        [Header("Sincronización")]
        private NetworkVariable<int> activeSkinIndex = new NetworkVariable<int>(0, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);

        private GameObject _currentVisualInstance;
        private Coroutine _skinCoroutine;

        private void Awake()
        {
            if (skinContainer == null) skinContainer = transform;
        }

        private void Start()
        {
            // Soporte para Modo Práctica / Offline
            if (!IsSpawned && availableSkins.Count > 0)
            {
                ApplySkin(Random.Range(0, availableSkins.Count));
                Debug.Log("[SkinManager] Modo Offline detectado. Activando skin local.");
            }
        }

        public override void OnNetworkSpawn()
        {
            activeSkinIndex.OnValueChanged += OnSkinChanged;
            
            if (IsServer)
            {
                if (availableSkins.Count > 0)
                {
                    activeSkinIndex.Value = Random.Range(0, availableSkins.Count);
                    // IMPORTANTE: El servidor necesita aplicar su propia skin manualmente
                    ApplySkin(activeSkinIndex.Value);
                }
            }
            else
            {
                // Para clientes que se unen tarde
                ApplySkin(activeSkinIndex.Value);
            }
        }

        public override void OnNetworkDespawn()
        {
            activeSkinIndex.OnValueChanged -= OnSkinChanged;
        }

        private void OnSkinChanged(int oldIndex, int newIndex)
        {
            ApplySkin(newIndex);
        }

        public void ApplySkin(int index)
        {
            if (index < 0 || index >= availableSkins.Count) return;

            if (gameObject.activeInHierarchy)
            {
                if (_skinCoroutine != null) StopCoroutine(_skinCoroutine);
                _skinCoroutine = StartCoroutine(ApplySkinCoroutine(availableSkins[index]));
            }
        }

        private IEnumerator ApplySkinCoroutine(CharacterSkin skinData)
        {
            if (skinData == null || skinData.prefab == null) yield break;

            Animator anim = GetComponent<Animator>();
            if (anim == null) yield break;

            // Guardar estado actual de la animación
            float speed = anim.GetFloat("Speed");
            bool isFalling = anim.GetBool("IsFalling");
            anim.enabled = false;

            // 1. Limpiar visual anterior
            if (_currentVisualInstance != null)
            {
                Destroy(_currentVisualInstance);
            }

            // 2. Instanciar nueva skin
            _currentVisualInstance = Instantiate(skinData.prefab, skinContainer);
            _currentVisualInstance.name = skinData.targetRootName; // Coincidir con la ruta de la animación
            _currentVisualInstance.transform.localScale = Vector3.one * skinData.modelScale;

            // 3. UNIFICAR JERARQUÍA (Runtime Bone Unification)
            // Buscamos el primer hijo (que debería ser el root del esqueleto) y lo renombreamos
            if (_currentVisualInstance.transform.childCount > 0)
            {
                Transform rootBone = _currentVisualInstance.transform.GetChild(0);
                if (rootBone.name != skinData.targetRootName)
                {
                    Debug.Log($"[SkinManager] Unificando jerarquía: {rootBone.name} -> {skinData.targetRootName}");
                    rootBone.name = skinData.targetRootName;
                }
            }

            // Esperar un frame para que Unity registre cambios de jerarquía
            yield return null;

            // 4. Vincular Animator
            anim.avatar = skinData.avatar;
            anim.enabled = true;
            anim.Rebind();

            // Restaurar parámetros
            anim.SetFloat("Speed", speed);
            anim.SetBool("IsFalling", isFalling);
            anim.Update(0);

            Debug.Log($"[SkinManager] Skin '{skinData.skinName}' aplicada con éxito.");
            _skinCoroutine = null;
        }
    }
}
