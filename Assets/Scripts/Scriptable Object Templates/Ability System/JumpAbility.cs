using UnityEngine;

namespace HauntedHouses.Scriptable_Object_Templates.Ability_System
{
    [CreateAssetMenu(fileName = "Ability", menuName = "ScriptableObjects/AbilitySystem/Jump")]
    public class JumpAbility : BaseAbility
    {
        private Vector3 _velocity;
        [SerializeField] private float jumpForce;

        public override void Activate(AbilityHolder holder)
        {
            PlayerController playerController = holder.PlayerController;
            //_velocity.y = Mathf.Sqrt(jumpForce * -2f * gravity);

            if (playerController != null)
            {
                float jumpVelocity = Mathf.Sqrt(jumpForce * -2f * playerController.Gravity);
                Debug.Log(jumpVelocity);
                playerController.ApplyVerticalForce(jumpVelocity);
                Debug.Log("Ability сработала");
                
                //надо как-то имитировать распрыг как в кс. возможно, если ты касаешься поверхности менее 0.3 сек
                //и отпрыгиваешь - дальность прыжка умножается на 2
            }
            else
            {
                Debug.LogError("PlayerController not found via AbilityHolder. Check AbilityHolder's Awake method.",
                    holder.Owner);
            }
        }
    }
}
    
