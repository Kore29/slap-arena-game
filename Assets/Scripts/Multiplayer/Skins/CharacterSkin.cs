using UnityEngine;

namespace SlapArena.Multiplayer
{
    [CreateAssetMenu(fileName = "New Skin", menuName = "SlapArena/Character Skin")]
    public class CharacterSkin : ScriptableObject
    {
        public string skinName;
        public GameObject prefab;
        public Avatar avatar;
        public float modelScale = 0.5f;
        
        [Header("Estructura de Huesos")]
        [Tooltip("Si el modelo tiene un nombre de hueso raíz distinto (ej: Armature), el sistema lo renombrará a este estándar.")]
        public string targetRootName = "CharacterArmature";
    }
}
