using UnityEngine;

namespace FireSafety
{
    public sealed class SmokeDetector : MonoBehaviour
    {
        public float threshold = .065f;
        public bool inPassage;
        public Renderer indicator;
        public FireAlarmController alarm;
        public float Reading { get; private set; }
        public bool Activated { get; private set; }
        public float ActivatedAt { get; private set; } = -1;
        MaterialPropertyBlock block;
        public void Step(float concentration, float dt, float time)
        {
            Reading = Mathf.Lerp(Reading, concentration, 1 - Mathf.Exp(-dt / 2));
            if (!Activated && Reading >= threshold) {
                Activated = true; ActivatedAt = time;
                if (alarm) alarm.Activate(name, time);
            }
        }
        public void ResetDetector() { Reading = 0; Activated = false; ActivatedAt = -1; }
        void LateUpdate()
        {
            if (!indicator) return;
            block ??= new MaterialPropertyBlock();
            Color color = Activated ? new Color(1, .035f, .01f) : new Color(.02f, .5f, .2f);
            block.SetColor("_BaseColor", color); block.SetColor("_EmissionColor", color * (Activated ? 2 : .3f));
            indicator.SetPropertyBlock(block);
        }
    }
}
