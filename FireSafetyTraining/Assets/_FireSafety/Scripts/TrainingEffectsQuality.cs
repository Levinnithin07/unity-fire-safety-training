using System;
using UnityEngine;

namespace FireSafety
{
    public sealed class TrainingEffectsQuality : MonoBehaviour
    {
        [Serializable] public struct Budget
        {
            public ParticleSystem particles;
            [Min(1)] public int highLimit;
        }
        public Budget[] budgets;
        public int AppliedQuality { get; private set; } = 1;
        public int TotalParticleLimit { get; private set; }

        public void Apply(int quality)
        {
            AppliedQuality = Mathf.Clamp(quality, 0, 2);
            float scale = AppliedQuality == 0 ? .55f : AppliedQuality == 1 ? .8f : 1f;
            TotalParticleLimit = 0;
            if (budgets == null) return;
            foreach (var budget in budgets) {
                if (!budget.particles) continue;
                int limit = Mathf.Max(8, Mathf.CeilToInt(budget.highLimit * scale));
                var main = budget.particles.main;
                main.maxParticles = limit;
                TotalParticleLimit += limit;
            }
            // Only rendering budgets change. Heat, smoke hazards, hit tests and suppression do not.
        }
    }
}
