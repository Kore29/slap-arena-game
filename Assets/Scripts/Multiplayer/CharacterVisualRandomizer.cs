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
            
            UpdateVisuals(0, visualIndex.Value);
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
            if (visuals == null || visuals.Length == 0) return;

            // Ocultar todos y activar solo el nuevo
            for (int i = 0; i < visuals.Length; i++)
            {
                if (visuals[i] != null)
                {
                    visuals[i].SetActive(i == newIndex);
                }
            }

            // CAMBIO CLAVE: Actualizar el Avatar del Animator (Task 3.3)
            Animator anim = GetComponent<Animator>();
            if (anim != null && avatars != null && newIndex < avatars.Length)
            {
                if (avatars[newIndex] != null)
                {
                    // Desactivar y activar el Animator suele "forzar" que el avatar se asiente bien
                    anim.enabled = false;
                    anim.avatar = avatars[newIndex];
                    anim.enabled = true;
                    
                    anim.Rebind(); 
                    anim.Update(0); // Forzar un frame de actualización
                    Debug.Log($"[Randomizer] Cambiada skin a {newIndex} y asignado Avatar: {avatars[newIndex].name}");
                }
            }
        }
    }
}
