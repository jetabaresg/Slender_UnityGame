using UnityEngine;
using ElmanGameDevTools.PlayerSystem;

namespace ElmanGameDevTools.PlayerAudio
{
    /// <summary>
    /// Manages player footstep sounds based on terrain textures or object tags.
    /// Includes smoothing for stairs to prevent audio cutting.
    /// </summary>
    public class PlayerMusic : MonoBehaviour
    {
        [System.Serializable]
        public struct SurfaceSetting
        {
            public string tag;
            public Texture2D texture;
            public AudioClip clip;
        }

        [Header("SURFACE CONFIG")]
        public SurfaceSetting[] surfaceSettings;
        public AudioClip defaultClip;

        [Header("AUDIO SETTINGS")]
        public AudioSource audioSource;
        [Range(0f, 1f)] public float volume = 0.5f;

        [Header("PITCH (SPEED)")]
        public float walkPitch = 1.0f;
        public float runPitch = 1.3f;
        public float crouchPitch = 0.8f;

        [Header("REFERENCES")]
        public PlayerController playerController;

        private float _airTimeThreshold = 0.2f;
        private float _currentAirTime;

        private void Start()
        {
            if (audioSource == null) audioSource = gameObject.AddComponent<AudioSource>();
            if (playerController == null) playerController = GetComponentInParent<PlayerController>();
            audioSource.loop = true;
        }

        private void Update()
        {
            if (playerController == null) return;

            // Reverted to standard GetAxis for movement detection
            bool isMoving = new Vector2(Input.GetAxis("Horizontal"), Input.GetAxis("Vertical")).magnitude > 0.1f;

            if (!playerController.IsGrounded)
                _currentAirTime += Time.deltaTime;
            else
                _currentAirTime = 0f;

            bool shouldPlay = (_currentAirTime < _airTimeThreshold) && isMoving;

            if (shouldPlay)
                PlaySteps();
            else if (audioSource.isPlaying)
                audioSource.Stop();
        }

        private void PlaySteps()
        {
            AudioClip clip = GetClip();

            if (clip != null)
            {
                if (!audioSource.isPlaying || audioSource.clip != clip)
                {
                    audioSource.clip = clip;
                    audioSource.Play();
                }
            }

            float targetPitch = walkPitch;
            if (playerController.IsCrouching) targetPitch = crouchPitch;
            else if (playerController.CurrentState == PlayerController.MovementState.Running) targetPitch = runPitch;

            audioSource.pitch = Mathf.Lerp(audioSource.pitch, targetPitch, Time.deltaTime * 10f);
            audioSource.volume = volume;
        }

        private AudioClip GetClip()
        {
            RaycastHit hit;
            if (Physics.Raycast(transform.position + Vector3.up * 0.1f, Vector3.down, out hit, 1.5f))
            {
                Terrain terrain = hit.collider.GetComponent<Terrain>();
                if (terrain != null)
                {
                    Texture2D currentTerrainTex = GetTerrainTexture(terrain, hit.point);
                    if (currentTerrainTex != null)
                    {
                        foreach (var surface in surfaceSettings)
                            if (surface.texture == currentTerrainTex) return surface.clip;
                    }
                }

                foreach (var surface in surfaceSettings)
                {
                    if (!string.IsNullOrEmpty(surface.tag) && hit.collider.CompareTag(surface.tag))
                        return surface.clip;
                }
            }
            return defaultClip;
        }

        /// <summary>
        /// Gets the dominant texture of a terrain at a specific world position.
        /// </summary>
        private Texture2D GetTerrainTexture(Terrain terrain, Vector3 worldPos)
        {
            TerrainData terrainData = terrain.terrainData;
            Vector3 terrainPos = terrain.transform.position;
            int mapX = (int)(((worldPos.x - terrainPos.x) / terrainData.size.x) * terrainData.alphamapWidth);
            int mapZ = (int)(((worldPos.z - terrainPos.z) / terrainData.size.z) * terrainData.alphamapHeight);
            float[,,] splatmapData = terrainData.GetAlphamaps(mapX, mapZ, 1, 1);

            int maxIndex = 0;
            float maxWeight = 0;
            for (int i = 0; i < terrainData.terrainLayers.Length; i++)
            {
                if (splatmapData[0, 0, i] > maxWeight)
                {
                    maxWeight = splatmapData[0, 0, i];
                    maxIndex = i;
                }
            }
            return terrainData.terrainLayers[maxIndex].diffuseTexture;
        }
    }
}