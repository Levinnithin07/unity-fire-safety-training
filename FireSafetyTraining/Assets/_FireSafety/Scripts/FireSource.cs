using UnityEngine;

namespace FireSafety
{
    public sealed class FireSource : MonoBehaviour
    {
        [Range(0, 1)] public float intensity;
        public float growthPerSecond = .022f;
        public float fuel = 1;
        public FireSource[] spreadTargets;
        public ParticleSystem flames, smoke, embers;
        public Light fireLight;
        public AudioSource crackle;
        public float IgnitedAt { get; private set; } = -1;
        public bool Burning => intensity > .001f && fuel > 0;
        public float Exposure { get; private set; }
        public float WaterApplied { get; private set; }
        public float AgentApplied { get; private set; }
        public bool ExtinguishedByAgent { get; private set; }
        readonly ParticleSystem.Particle[] smokeParticles = new ParticleSystem.Particle[100];

        public void Ignite(float time)
        {
            if (Burning || fuel <= 0) return;
            intensity = .18f; IgnitedAt = time; Exposure = 0;
        }
        public void Expose(float heat, float dt, float time)
        {
            if (Burning || IgnitedAt >= 0) return;
            Exposure = Mathf.Max(0, Exposure + (heat - .045f) * dt);
            if (Exposure >= 1.2f) Ignite(time);
        }
        public void Step(float dt, float suppression)
        {
            if (!Burning) return;
            WaterApplied += suppression * dt;
            fuel = Mathf.Max(0, fuel - intensity * .0015f * dt);
            intensity = Mathf.Clamp01(intensity + (growthPerSecond * fuel - suppression) * dt);
            if (intensity < .012f || fuel <= 0) intensity = 0;
        }
        public void ResetFire()
        {
            intensity = 0; fuel = 1; IgnitedAt = -1; Exposure = WaterApplied = 0;
            AgentApplied = 0; ExtinguishedByAgent = false;
            foreach (var p in new[] { flames, smoke, embers }) if (p) p.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
            if (crackle) crackle.Stop();
            if (fireLight) fireLight.enabled = false;
        }
        public float ApplyExtinguishingAgent(float amount)
        {
            if (!Burning || amount <= 0) return 0;
            float before = intensity;
            intensity = Mathf.Max(0, intensity - amount);
            if (intensity < .012f) { intensity = 0; ExtinguishedByAgent = true; }
            AgentApplied += before - intensity;
            return before - intensity;
        }
        static void Emission(ParticleSystem ps, float rate)
        {
            if (!ps) return;
            var emission = ps.emission; emission.rateOverTime = rate;
            if (rate > 0 && !ps.isPlaying) ps.Play();
            else if (rate <= 0 && ps.isEmitting) ps.Stop(false, ParticleSystemStopBehavior.StopEmitting);
        }
        void LateUpdate()
        {
            float strength = Burning ? intensity : 0;
            Emission(flames, strength * 48); Emission(smoke, strength * 8); Emission(embers, strength * 5);
            if (flames) { var main = flames.main; main.startSize = new ParticleSystem.MinMaxCurve(.10f + strength * .28f, .16f + strength * .70f); }
            if (fireLight) { fireLight.enabled = Burning; fireLight.intensity = strength * (.85f + Mathf.PerlinNoise(Time.time * 9, transform.position.z) * .55f); }
            if (crackle) { crackle.volume = strength * .32f; if (Burning && !crackle.isPlaying) crackle.Play(); else if (!Burning && crackle.isPlaying) crackle.Stop(); }
            // Keep the simplified smoke plume below the ceiling instead of leaking through the roof.
            if (smoke && smoke.particleCount > 0) {
                int n = smoke.GetParticles(smokeParticles);
                for (int i = 0; i < n; i++) if (smokeParticles[i].position.y > 2.65f) {
                    var p = smokeParticles[i]; var pos = p.position; pos.y = 2.65f;
                    p.position = pos; p.velocity = new Vector3(.10f, 0, .04f); smokeParticles[i] = p;
                }
                smoke.SetParticles(smokeParticles, n);
            }
        }
    }
}
