using UnityEngine;

namespace FireSafety
{
    // Presentation only: neither footsteps nor the success cue can affect training scores.
    [DefaultExecutionOrder(400)]
    public sealed class TrainingFeedback : MonoBehaviour
    {
        public TrainingSession session;
        public DesktopPlayerController player;
        public FireExtinguisherController extinguisher;
        public AudioSource footsteps, completion;
        public AudioClip[] footstepClips;
        public AudioClip completionClip;
        public int StepsPlayed { get; private set; }
        public int CompletionCuesPlayed { get; private set; }
        public float LastStepVolume { get; private set; }
        Vector3 previousPosition;
        float distanceSinceStep;
        bool completionPlayed;

        void OnEnable() { if (player) previousPosition = player.transform.position; }
        void LateUpdate()
        {
            if (!session || !player) return;
            var position = player.transform.position;
            var delta = position - previousPosition;
            previousPosition = position;
            delta.y = 0;
            if (!session.HasSession || !extinguisher.SuppressionCompleted) completionPlayed = false;
            if (!session.HasSession) completion.Stop();
            if (!session.Playing || !player.Captured) {
                distanceSinceStep = 0;
                footsteps.Stop();
                return;
            }

            // Measure collision-resolved movement, not held keys. Ignore spawn/teleport jumps.
            float distance = delta.magnitude;
            if (!player.Grounded || distance < .0005f || distance > .5f) distanceSinceStep = 0;
            else {
                distanceSinceStep += distance;
                float stride = player.IsCrouching ? .55f : player.LastSprintInput ? .95f : .75f;
                if (distanceSinceStep >= stride && footstepClips.Length > 0) {
                    distanceSinceStep %= stride;
                    LastStepVolume = player.IsCrouching ? .07f : player.LastSprintInput ? .22f : .16f;
                    footsteps.pitch = .97f + (StepsPlayed % 3) * .03f;
                    footsteps.PlayOneShot(footstepClips[StepsPlayed % footstepClips.Length], LastStepVolume);
                    StepsPlayed++;
                }
            }

            if (session.IsExtinguisherMode && extinguisher.SuppressionCompleted && !completionPlayed) {
                completionPlayed = true;
                completion.PlayOneShot(completionClip, .26f);
                CompletionCuesPlayed++;
            }
        }
        void OnDisable()
        {
            if (footsteps) footsteps.Stop();
            if (completion) completion.Stop();
            distanceSinceStep = 0;
        }
    }
}
