using UnityEngine;
using System;
/*

Character Animation controller
FIXME rework to C# docstring
*/
public class CharacterAnimationControllerBACKUP : MonoBehaviour
{
    private Animator animator;

    private void Start()
    {
        this.animator = GetComponent<Animator>();
    }

    /*
        Handles walking animations.
        Does nothing when character is idle.
    */
    public void CharacterMovement(Vector3 moveDir){
        bool isIdle = moveDir.x == 0 && moveDir.y == 0;

        if (isIdle){
            return;
        }else{
            QuadrantRotation(moveDir,"walk");
        }

    }
    /*
        Handles idle status animations.
        Turns character to corresponding position.
    */
    public void CharacterDirection(Vector3 lookingDir){
           QuadrantRotation(lookingDir,"idle");
    }

    /*
        Calculates angle between horizontal X axis
        and given normalized looking mouse position vector

       @ returns angle
    */
    private float AxisAngleX(float x, Vector3 lookingDir){
            var v = new Vector3(x,0f,0f); //positive x axis vector

            float angle = Vector3.Angle(lookingDir, v);
            return angle;
    }

    /*
        Picks action animation according to
        given direction and action.
        Each animation has 4-way direction
    */

    private void QuadrantRotation(Vector3 direction, string action){
            //1st quadrant
            if (direction.x > 0 && direction.y > 0){
                float angle = AxisAngleX(1.0f, direction);
                if (angle > 45f) animator.Play("up_"+action); 
                else animator.Play("right_"+action); 
            }
            //4th quadrant
            if (direction.x > 0 && direction.y < 0){
                float angle = AxisAngleX(1.0f, direction);
                if (angle < 45f) animator.Play("right_"+action);
                else animator.Play("down_"+action);
            }
            //2nd quadrant
            if (direction.x < 0 && direction.y > 0){
                float angle = AxisAngleX(-1.0f, direction);
                if (angle > 45f) animator.Play("up_"+action); 
                else animator.Play("left_"+action);
            }
            //3nd quadrant
            if (direction.x < 0 && direction.y < 0){
                float angle = AxisAngleX(-1.0f, direction);
                if (angle > 45f) animator.Play("down_"+action); 
                else animator.Play("left_"+action);
            }
    }
}
