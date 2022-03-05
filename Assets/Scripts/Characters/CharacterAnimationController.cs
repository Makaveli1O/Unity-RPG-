using UnityEngine;
using System;
using Random = UnityEngine.Random;
/*

Character Animation controller

*/
public class CharacterAnimationController : MonoBehaviour
{
    private Animator animator;

    private void Start()
    {
        this.animator = GetComponent<Animator>();
    }

    /// <summary>
    /// Handles walking animation. Does nothing when character is idle.
    /// </summary>
    /// <param name="moveDir">Movement direction</param>
    /// <param name="twoDir">Boolean for two directional assets.</param>
    public void CharacterMovement(Vector3 moveDir, bool twoDir = false){
        bool isIdle = moveDir.x == 0 && moveDir.y == 0;

        if (isIdle){
            return;
        }else{
            if (!twoDir)    QuadrantRotation(moveDir,"walk");
            else QuadrantRotation2Dir(moveDir, "walk");
            
        }

    }

    /// <summary>
    /// Handles idle status animations. Turns characteer to corresponding position
    /// </summary>
    /// <param name="lookingDir">Movement direction</param>
    /// <param name="twoDir">Boolean for two directional assets.</param>
    public void CharacterDirection(Vector3 lookingDir, bool twoDir = false){
        if (!twoDir)    QuadrantRotation(lookingDir,"idle");
        else QuadrantRotation2Dir(lookingDir,"idle");
        
    }

    
    /// <summary>
    /// Handles attacking animations
    /// </summary>
    /// <param name="attackDir">direction to perform animation</param>
    /// <param name="onAnimationComplete">Animation complete callback not to interfere animation.</param>
    /// <param name="twoDir">Boolean for two directional assets.</param>
    public void CharacterAttack(Vector3 attackDir, bool twoDir = false){
        if(!twoDir) QuadrantRotation(attackDir, "attack");
        else  QuadrantRotation2Dir(attackDir, "attack");
        return;
    }
    
    /// <summary>
    /// Handles dead animation
    /// </summary>
    /// <param name="direction">Direction looking towards</param>
    /// <param name="twoDir">Boolean for two directional assets.</param>
    public void DeadAnimation(Vector3 direction, bool twoDir = false){
        //no move dir, pick random
        if(direction.Equals(Vector3.zero)) direction = new Vector3(Random.Range(-1f, 1f), Random.Range(-1f, 1f));
        if(!twoDir) QuadrantRotation(direction, "dead");
        else QuadrantRotation2Dir(direction, "dead");
        return;
    }

    /// <summary>
    /// Calculates angle between horizontal X axis and given normalized looking mouse position vector
    /// </summary>
    /// <param name="x">X point</param>
    /// <param name="lookingDir">Looking vector</param>
    /// <returns>Angle between two vectors</returns>
    private float AxisAngleX(float x, Vector3 lookingDir){
            var v = new Vector3(x,0f,0f); //positive x axis vector

            float angle = Vector3.Angle(lookingDir, v);
            return angle;
    }


    /// <summary>
    ///    Picks action animation according to
    ///    given direction and action.
    ///    Each animation has 4-way direction
    /// </summary>
    /// <param name="direction">Direction vector</param>
    /// <param name="action">Action type</param>
    /// <param name="onAnimComplete">Callback action</param>
    private void QuadrantRotation(Vector3 direction, string action, Action onAnimComplete = null){
            //1st quadrant
            if (direction.x > 0 && direction.y >= 0){
                float angle = AxisAngleX(1.0f, direction);
                if (angle > 45f) animator.Play("up_"+action); 
                else animator.Play("right_"+action); 
            }
            //4th quadrant
            if (direction.x >= 0 && direction.y < 0){
                float angle = AxisAngleX(1.0f, direction);
                if (angle < 45f) animator.Play("right_"+action);
                else animator.Play("down_"+action);                
            }
            //2nd quadrant
            if (direction.x <= 0 && direction.y > 0){
                float angle = AxisAngleX(-1.0f, direction);
                if (angle > 45f) animator.Play("up_"+action); 
                else animator.Play("left_"+action);
            }
            //3nd quadrant
            if (direction.x < 0 && direction.y <= 0){
                float angle = AxisAngleX(-1.0f, direction);
                if (angle > 45f) animator.Play("down_"+action); 
                else animator.Play("left_"+action);
            }
    }

    /// <summary>
    /// Quadrant rotation for sprites with only 2 way animations.
    /// </summary>
    /// <param name="direction">Direction vector</param>
    /// <param name="action">Action type</param>
    /// <param name="onAnimComplete">Callback action</param>
    private void QuadrantRotation2Dir(Vector3 direction, string action){
            //1st quadrant
            if (direction.x > 0 && direction.y >= 0){
                animator.Play("right_"+action); 
            }
            //4th quadrant
            if (direction.x >= 0 && direction.y < 0){
                animator.Play("right_"+action);             
            }
            //2nd quadrant
            if (direction.x <= 0 && direction.y > 0){
                animator.Play("left_"+action);
            }
            //3nd quadrant
            if (direction.x < 0 && direction.y <= 0){
                animator.Play("left_"+action);
            }
    }
}
