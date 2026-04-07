using UnityEngine;
using Unity.Netcode;
using System.Collections.Generic;

namespace SlapArena.Multiplayer
{
    /// <summary>
    /// Sincroniza visuales aleatorias y sus respectivos Avatars para animaciones Generic.
    /// </summary>
    public class CharacterVisualRandomizer : NetworkBehaviour
    {
        [Header("Configuración Visual")]
        public GameObject[] visuals; // Lista de los 5 modelos de conejo
        public Avatar[] avatars;     // Lista de los 5 Avatars (en el mismo orden)

        // Sincronización de red para el índice visual
        private NetworkVariable<int> visualIndex = new NetworkVariable<int>(0, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);

        private Coroutine _activeCoroutine;

        private void Awake()
        {
            HideAllModels();
        }

        private void Start()
        {
            // MODO ENTRENAMIENTO/LOCAL: Si no hay red, forzamos un visual aquí
            if (!IsSpawned && visuals != null && visuals.Length > 0)
            {
                UpdateVisuals(0, Random.Range(0, visuals.Length));
                Debug.Log("[Randomizer] Modo Entrenamiento detectado. Activando skin local.");
            }
        }

        public override void OnNetworkSpawn()
        {
            // Suscribirse al cambio de skin
            visualIndex.OnValueChanged += UpdateVisuals;
            
            if (IsServer)
            {
                if (visuals != null && visuals.Length > 0)
                {
                    visualIndex.Value = Random.Range(0, visuals.Length);
                }
            }
            else
            {
                // Modo local/entrenamiento o cliente recién unido
                if (visuals != null && visuals.Length > 0)
                {
                    // Si no estamos en red, elegimos uno al azar localmente
                    if (!IsClient && !IsServer) {
                        UpdateVisuals(0, Random.Range(0, visuals.Length));
                    } else {
                        // Forzar update inicial con el valor de red
                        UpdateVisuals(0, visualIndex.Value);
                    }
                }
            }
            
            // ELIMINADA: La llamada manual UpdateVisuals(0, visualIndex.Value);
            // El evento OnValueChanged ya se dispara al unirse o el if anterior lo maneja.
        }

        public override void OnNetworkDespawn()
        {
            visualIndex.OnValueChanged -= UpdateVisuals;
        }

        private void HideAllModels()
        {
            if (visuals == null) return;
            foreach (var model in visuals)
            {
                if (model != null) model.SetActive(false);
            }
        }

        private void UpdateVisuals(int oldIndex, int newIndex)
        {
            if (gameObject.activeInHierarchy)
            {
                if (_activeCoroutine != null) StopCoroutine(_activeCoroutine);
                _activeCoroutine = StartCoroutine(ApplyVisualsCoroutine(newIndex));
            }
        }

        private System.Collections.IEnumerator ApplyVisualsCoroutine(int index)
        {
            if (visuals == null || visuals.Length == 0) yield break;

            Animator anim = GetComponent<Animator>();
            if (anim == null) yield break;

            float currentSpeed = anim.GetFloat("Speed");
            bool currentFalling = anim.GetBool("IsFalling");

            // 1. DESACTIVAR Y RESETEAR NOMBRES
            anim.enabled = false;
            for (int i = 0; i < visuals.Length; i++)
            {
                if (visuals[i] != null) 
                {
                    visuals[i].SetActive(false);
                    // Restaurar nombre original para no tener duplicados
                    if (visuals[i].name == "CharacterModel") visuals[i].name = visuals[i].name.Replace("CharacterModel", $"Visual_{i}");
                }
            }

            // 2. ACTIVAR Y RENOMBRAR EL SELECCIONADO
            // Esto es vital: el Animator encontrará los huesos si el objeto padre se llama siempre igual
            GameObject selected = visuals[index];
            selected.name = "CharacterModel";
            selected.SetActive(true);

            // Esperar un frame para que Unity registre el cambio de jerarquía
            yield return null;

            // 3. RE-VINCULACIÓN DIRECTA
            if (avatars != null && index < avatars.Length && avatars[index] != null)
            {
                anim.avatar = avatars[index];
                anim.enabled = true;
                anim.Rebind();
                
                // Restaurar parámetros inmediatamente
                anim.SetFloat("Speed", currentSpeed);
                anim.SetBool("IsFalling", currentFalling);
                anim.Update(0);
                
                Debug.Log($"[Randomizer] MASTER FIX - Skin {index} renamed to CharacterModel | Avatar: {avatars[index].name}");
            }

            _activeCoroutine = null;
        }
    }
}
