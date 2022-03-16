using UnityEngine;

//TODO comments
public class CharacterMovementController : MonoBehaviour
{

    private const int noMovementFrames = 3;
    private Vector3[] previousLocations = new Vector3[noMovementFrames];
    private bool isMoving;

    public bool IsMoving{
        get{ return isMoving; }
    }

    //TODO docs
    public void characterPathExpand(){
        for(int i =0; i < previousLocations.Length - 1; i++){
            previousLocations[i] = previousLocations[i+1];
        }
        previousLocations[previousLocations.Length - 1] = this.transform.position;
    }

    //TODO docs
    public void characterMovementDetection(){
        float movementTreshold = 0.0001f;

        for(int i = 0; i < previousLocations.Length - 1; i++){
            if(Vector3.Distance(previousLocations[i], previousLocations[i+1]) <= movementTreshold){
                this.isMoving = false;
            }else{
                this.isMoving = true;
            }
        }
    }
}
