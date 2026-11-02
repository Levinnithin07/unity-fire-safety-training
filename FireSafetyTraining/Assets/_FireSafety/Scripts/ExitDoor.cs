using UnityEngine;
using UnityEngine.InputSystem;
namespace FireSafety
{
    public sealed class ExitDoor : MonoBehaviour
    {
        public DesktopPlayerController player;
        public Transform leftLeaf, rightLeaf;
        public AudioSource mechanism;
        public bool OpenRequested { get; private set; }
        public bool IsPassable => progress > .88f;
        public bool CanInteract { get; private set; }
        float progress;
        InputAction interact;
        void Awake(){interact=new InputAction("Open exit",InputActionType.Button,"<Keyboard>/e");}
        void OnEnable()=>interact?.Enable();
        void OnDisable()=>interact?.Disable();
        void OnDestroy()=>interact?.Dispose();
        public bool CheckInteraction(Ray ray)
        {
            return !OpenRequested&&Physics.Raycast(ray,out var hit,2.3f,1,QueryTriggerInteraction.Ignore)&&hit.transform.IsChildOf(transform);
        }
        public bool TryOpen(Ray ray)
        {
            if(!CheckInteraction(ray))return false;
            OpenRequested=true;if(mechanism)mechanism.Play();return true;
        }
        public bool RequestOpenNearby(Vector3 requester)
        {
            if(Vector3.Distance(requester,transform.position)>2.5f)return false;
            if(!OpenRequested){OpenRequested=true;if(mechanism)mechanism.Play();}
            return true;
        }
        public void ResetDoor(){OpenRequested=false;progress=0;CanInteract=false;leftLeaf.localRotation=rightLeaf.localRotation=Quaternion.identity;if(mechanism)mechanism.Stop();}
        void Update()
        {
            CanInteract=player.Captured&&CheckInteraction(new Ray(player.view.position,player.view.forward));
            if(CanInteract&&interact.WasPressedThisFrame())TryOpen(new Ray(player.view.position,player.view.forward));
            progress=Mathf.MoveTowards(progress,OpenRequested?1:0,Time.deltaTime*1.5f);
            leftLeaf.localRotation=Quaternion.Euler(0,95*Mathf.SmoothStep(0,1,progress),0);
            rightLeaf.localRotation=Quaternion.Euler(0,-95*Mathf.SmoothStep(0,1,progress),0);
        }
    }
}
