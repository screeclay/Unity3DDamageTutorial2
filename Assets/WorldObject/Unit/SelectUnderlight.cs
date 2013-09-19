using UnityEngine;


public class SelectUnderlight : MonoBehaviour {
		
	private float Range ;
	
	protected void Start () {
		Light SelectUnderlight = GetComponent<Light>();
        Range = SelectUnderlight.range;
		SelectUnderlight.range = 0;//turning off
	}	
	
	protected void Update () {
					
	}
	
	public void Lighten(bool variable){
		Light SelectUnderlight = GetComponent<Light>();
		if(variable){
			SelectUnderlight.range=Range;
		}
		else{
			SelectUnderlight.range = 0;
		}	
	
	}
	
	public void SetRange(float range){
		Range = range;
	}	
}