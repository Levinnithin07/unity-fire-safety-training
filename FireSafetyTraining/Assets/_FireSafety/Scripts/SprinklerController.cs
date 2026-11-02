using UnityEngine;

namespace FireSafety
{
    public sealed class SprinklerController : MonoBehaviour
    {
        public float activationTemperature = 68;
        public float thermalResponseSeconds = 6;
        public float coverageRadius = 1.65f;
        public float suppressionPerSecond = .062f;
        public ParticleSystem water;
        public AudioSource waterSound;
        public GameObject glassBulb;
        public float Temperature { get; private set; } = 22;
        public bool Active { get; private set; }
        public float ActivatedAt { get; private set; } = -1;
        public bool Available { get; private set; }=true;
        public void SetAvailable(bool value){ResetSprinkler();Available=value;enabled=value;}
        public void Step(float airTemperature, float dt, float time)
        {
            if(!Available)return;
            Temperature = Mathf.Lerp(Temperature, airTemperature, 1 - Mathf.Exp(-dt / thermalResponseSeconds));
            if (!Active && Temperature >= activationTemperature) {
                Active = true; ActivatedAt = time;
                if (glassBulb) glassBulb.SetActive(false);
                if (water) water.Play();
                if (waterSound) waterSound.Play();
            }
        }
        public float SuppressionAt(Vector3 point)
        {
            if (!Available || !Active || point.y >= transform.position.y) return 0;
            var d = point - transform.position; d.y = 0;
            return d.magnitude < coverageRadius ? suppressionPerSecond * Mathf.Lerp(1, .65f, d.magnitude / coverageRadius) : 0;
        }
        public void ResetSprinkler()
        {
            Temperature = 22; Active = false; ActivatedAt = -1;
            if (glassBulb) glassBulb.SetActive(true);
            if (water) water.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
            if (waterSound) waterSound.Stop();
        }
    }
}
