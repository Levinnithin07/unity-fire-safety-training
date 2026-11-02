using UnityEngine;

namespace FireSafety
{
    public sealed class FireAlarmController : MonoBehaviour
    {
        public AudioSource sounder;
        public Light beacon;
        public Renderer lens;
        public bool Active { get; private set; }
        public bool Muted { get; private set; }
        public string TriggeredBy { get; private set; }
        public float ActivatedAt { get; private set; } = -1;
        MaterialPropertyBlock block;
        public void Activate(string detector, float time)
        {
            if (Active) return;
            Active = true; ActivatedAt = time; TriggeredBy = detector;
            if (sounder) sounder.Play();
        }
        public void ToggleMute() { Muted = !Muted; if (sounder) sounder.mute = Muted; }
        public void ResetAlarm()
        {
            Active = false; ActivatedAt = -1; TriggeredBy = "";
            if (sounder) sounder.Stop();
            if (beacon) beacon.enabled = false;
        }
        void LateUpdate()
        {
            bool pulse = Active && Time.time % 1.1f < .18f;
            if (beacon) beacon.enabled = pulse;
            if (lens) {
                block ??= new MaterialPropertyBlock();
                block.SetColor("_EmissionColor", pulse ? new Color(4, .12f, .04f) : Color.black);
                lens.SetPropertyBlock(block);
            }
        }
    }
}
