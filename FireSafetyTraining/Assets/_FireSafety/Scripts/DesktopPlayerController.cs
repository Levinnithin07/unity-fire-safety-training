using UnityEngine;
using UnityEngine.InputSystem;

namespace FireSafety
{
    [RequireComponent(typeof(CharacterController))]
    public sealed class DesktopPlayerController : MonoBehaviour
    {
        public Transform view;
        public float walkSpeed = 2.2f, sprintSpeed = 3.5f, crouchSpeed = 1.2f;
        public float standingHeight = 1.8f, crouchingHeight = 1.05f;
        public float mouseSensitivity = 0.085f;
        public LayerMask environmentMask = 1;
        public bool Captured { get; private set; }
        public int LastCaptureFrame { get; private set; } = -1;
        public bool IsCrouching { get; private set; }
        public bool Grounded => motor && motor.isGrounded;
        public Vector2 LastMoveInput { get; private set; }
        public bool LastSprintInput { get; private set; }
        public float Pitch => pitch;
        CharacterController motor;
        InputAction move, look, sprint, crouch, release, capture;
        float verticalSpeed, pitch;
        Vector2 lookVelocity;
        readonly Collider[] clearance = new Collider[16];

        void Awake()
        {
            motor = GetComponent<CharacterController>();
            mouseSensitivity = PlayerPrefs.GetFloat("Training.MouseSensitivity", mouseSensitivity);
            move = new InputAction("Move", InputActionType.Value);
            move.AddCompositeBinding("2DVector").With("Up", "<Keyboard>/w").With("Down", "<Keyboard>/s")
                .With("Left", "<Keyboard>/a").With("Right", "<Keyboard>/d");
            look = new InputAction("Look", InputActionType.Value, "<Mouse>/delta");
            sprint = new InputAction("Sprint", InputActionType.Button, "<Keyboard>/leftShift");
            crouch = new InputAction("Crouch", InputActionType.Button, "<Keyboard>/leftCtrl");
            release = new InputAction("Release cursor", InputActionType.Button, "<Keyboard>/escape");
            capture = new InputAction("Capture cursor", InputActionType.Button, "<Mouse>/leftButton");
        }
        void OnEnable() { foreach (var a in new[]{move,look,sprint,crouch,release,capture}) a?.Enable(); }
        void Start() => SetCapture(TrainingSession.InputAllowed);
        public void SetCapture(bool value)
        {
            if (value && !Captured) LastCaptureFrame = Time.frameCount;
            Captured = value;
            Cursor.lockState = value ? CursorLockMode.Locked : CursorLockMode.None;
            Cursor.visible = !value;
            lookVelocity = Vector2.zero;
        }
        void OnApplicationFocus(bool focus) { if (!focus) SetCapture(false); }
        void Update()
        {
            if(!TrainingSession.InputAllowed)return;
            if (!TrainingSession.Instance && release.WasPressedThisFrame()) SetCapture(false);
            else if (capture.WasPressedThisFrame() && !Captured) SetCapture(true);
            float dt = Mathf.Min(Time.deltaTime, 0.05f);
            LastMoveInput = Captured ? Vector2.ClampMagnitude(move.ReadValue<Vector2>(),1) : Vector2.zero;
            LastSprintInput = Captured && sprint.IsPressed();
            StepMotor(LastMoveInput, LastSprintInput, Captured && crouch.IsPressed(), dt);
            if (!Captured) return;
            Vector2 target = look.ReadValue<Vector2>() * mouseSensitivity;
            lookVelocity = Vector2.Lerp(lookVelocity,target,1-Mathf.Exp(-35*dt));
            transform.Rotate(0,lookVelocity.x,0);
            pitch = Mathf.Clamp(pitch-lookVelocity.y,-85,85);
            view.localRotation = Quaternion.Euler(pitch,0,0);
        }
        bool CanStand()
        {
            float radius=motor.radius-0.015f;
            var feet=transform.position;
            int count=Physics.OverlapCapsuleNonAlloc(feet+Vector3.up*radius,
                feet+Vector3.up*(standingHeight-radius),radius,clearance,environmentMask,QueryTriggerInteraction.Ignore);
            for(int i=0;i<count;i++) if(clearance[i] && clearance[i] != motor && !clearance[i].transform.IsChildOf(transform)) return false;
            return count < clearance.Length;
        }
        // Shared deterministic motor path also exercised by the Play Mode collision checks.
        public void StepMotor(Vector2 input,bool fast,bool duck,float dt)
        {
            if(!motor) motor=GetComponent<CharacterController>();
            if(!motor.enabled || dt<=0) return;
            IsCrouching=duck || (IsCrouching && !CanStand());
            float height=IsCrouching?crouchingHeight:standingHeight;
            motor.height=height;motor.center=Vector3.up*(height*.5f);
            var eye=view.localPosition;
            eye.y=Mathf.Lerp(eye.y,height-.13f,1-Mathf.Exp(-14*dt));view.localPosition=eye;
            if(motor.isGrounded && verticalSpeed<0) verticalSpeed=-2;
            verticalSpeed=Mathf.Max(verticalSpeed-18*dt,-25);
            float speed=IsCrouching?crouchSpeed:(fast?sprintSpeed:walkSpeed);
            Vector3 direction=transform.TransformDirection(new Vector3(input.x,0,input.y));
            var flags=motor.Move((Vector3.ClampMagnitude(direction,1)*speed+Vector3.up*verticalSpeed)*dt);
            if((flags&CollisionFlags.Above)!=0 && verticalSpeed>0) verticalSpeed=0;
        }
        public void SetSensitivity(float value)
        {
            mouseSensitivity=Mathf.Clamp(value,.02f,.3f);
            PlayerPrefs.SetFloat("Training.MouseSensitivity",mouseSensitivity);
        }
        public void ResetPose(Vector3 position)
        {
            bool enabledBefore=motor.enabled;motor.enabled=false;
            transform.SetPositionAndRotation(position,Quaternion.identity);pitch=verticalSpeed=0;lookVelocity=Vector2.zero;
            view.localRotation=Quaternion.identity;view.localPosition=new Vector3(0,standingHeight-.13f,0);
            IsCrouching=false;motor.height=standingHeight;motor.center=Vector3.up*(standingHeight*.5f);motor.enabled=enabledBefore;
            Physics.SyncTransforms();
        }
        void OnDisable()
        {
            foreach(var a in new[]{move,look,sprint,crouch,release,capture}) a?.Disable();
            SetCapture(false);
        }
        void OnDestroy() { foreach(var a in new[]{move,look,sprint,crouch,release,capture}) a?.Dispose(); }
    }
}
