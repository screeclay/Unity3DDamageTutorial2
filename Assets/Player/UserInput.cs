using UnityEngine;
using System.Collections;
using RTS;


public class UserInput : MonoBehaviour {
	
	private Player player;
	private SelectRectangle selectRectangle;
	private bool IsCameraMovingToObject = false;
	private Vector3 CameraTargetPosition;
	public float DistanceToStopMovingCamera = 20f;
	public float rotationSpeed = 3;
	public float CameraMovingToObjectSpeed = 0.10f;
	
	// Use this for initialization
	void Start () {
		player = transform.root.GetComponent<Player>();
		selectRectangle = transform.root.GetComponentInChildren<SelectRectangle>();
		CameraTargetPosition = Camera.mainCamera.transform.position;
	}
	
	// Update is called once per frame
	void Update () {
		if(player.human) {
			KeyboardActivity();
			MoveCamera();
			RotateCamera();
			MouseActivity();

		}
		
	}
	
	private void MoveCamera() {

		float xpos = Input.mousePosition.x;
		float ypos = Input.mousePosition.y;
		Vector3 movement = new Vector3(0,0,0);
		bool mouseScroll = false;
		
		//horizontal camera movement
		if(xpos >= 0 && xpos < ResourceManager.ScrollWidth) {
			movement.x -= ResourceManager.ScrollSpeed;
			player.hud.SetCursorState(CursorState.PanLeft);
			mouseScroll = true;
		} else if(xpos <= Screen.width && xpos > Screen.width - ResourceManager.ScrollWidth) {
			movement.x += ResourceManager.ScrollSpeed;
			player.hud.SetCursorState(CursorState.PanRight);
			mouseScroll = true;
		}
		
		//vertical camera movement
		if(ypos >= 0 && ypos < ResourceManager.ScrollWidth) {
			movement.z -= ResourceManager.ScrollSpeed;
			player.hud.SetCursorState(CursorState.PanDown);
			mouseScroll = true;
		} else if(ypos <= Screen.height && ypos > Screen.height - ResourceManager.ScrollWidth) {
			movement.z += ResourceManager.ScrollSpeed;
			player.hud.SetCursorState(CursorState.PanUp);
			mouseScroll = true;
		}
		if(mouseScroll){
				IsCameraMovingToObject = false; //if camera is moving, we want it to stop doing so
		}
		
		//make sure movement is in the direction the camera is pointing
		//but ignore the vertical tilt of the camera to get sensible scrolling
		movement = Camera.mainCamera.transform.TransformDirection(movement);
		movement.y = 0;
		
		//away from ground movement
		movement.y -= ResourceManager.ScrollSpeed * Input.GetAxis("Mouse ScrollWheel");
		
		//calculate desired camera position based on received input
		Vector3 origin = Camera.mainCamera.transform.position;
		Vector3 destination = origin;
		destination.x += movement.x;
		destination.y += movement.y;
		destination.z += movement.z;
		
		//limit away from ground movement to be between a minimum and maximum distance
		if(destination.y > ResourceManager.MaxCameraHeight) {
			destination.y = ResourceManager.MaxCameraHeight;
		} else if(destination.y < ResourceManager.MinCameraHeight) {
			destination.y = ResourceManager.MinCameraHeight;
		}
		
		//if a change in position is detected perform the necessary update
		if(destination != origin) {
			Camera.mainCamera.transform.position = Vector3.MoveTowards(origin, destination, Time.deltaTime * ResourceManager.ScrollSpeed);
		}
		
		//set cursor back to default state it should be in
		if(!mouseScroll) {
			player.hud.SetCursorState(CursorState.Select);
		}
		
		if(IsCameraMovingToObject){Debug.Log(Vector3.Distance(CameraTargetPosition,Camera.mainCamera.transform.position));	//moving to object
			/*if(Camera.mainCamera.transform.position.y<ResourceManager.MinCameraHeight){
				CameraTargetPosition.y = ResourceManager.MinCameraHeight;
			}*/
			
			if(Vector3.Distance(CameraTargetPosition,Camera.mainCamera.transform.position)<(DistanceToStopMovingCamera+destination.y)){
				IsCameraMovingToObject = false; 
			}else{
				Camera.mainCamera.transform.position += CameraMovingToObjectSpeed*(CameraTargetPosition-Camera.mainCamera.transform.position);
				//Camera.mainCamera.transform.rotation = Quaternion.Slerp(Camera.mainCamera.transform.rotation, Quaternion.LookRotation(CameraTargetPosition - Camera.mainCamera.transform.position), rotationSpeed*Time.deltaTime);

				
			}
		}
	}
	
	private void RotateCamera() {
		Vector3 origin = Camera.mainCamera.transform.eulerAngles;
		Vector3 destination = origin;
		
		//detect rotation amount if ALT is being held and the Right mouse button is down
		if((Input.GetKey(KeyCode.LeftAlt) || Input.GetKey(KeyCode.RightAlt)) && Input.GetMouseButton(1)) {
			destination.x -= Input.GetAxis("Mouse Y") * ResourceManager.RotateAmount;
			destination.y += Input.GetAxis("Mouse X") * ResourceManager.RotateAmount;
		}
		
		//if a change in position is detected perform the necessary update
		if(destination != origin) {
			Camera.mainCamera.transform.eulerAngles = Vector3.MoveTowards(origin, destination, Time.deltaTime * ResourceManager.RotateSpeed);
		}
	}
	
	private void MouseActivity() {
		if(Input.GetMouseButtonDown(0)) LeftMouseClick();
		else if(Input.GetMouseButtonUp(0)) LeftMouseUnClick();
		else if(Input.GetMouseButtonDown(1)) RightMouseClick();
		else if(Input.GetMouseButtonUp(1)) RightMouseUnClick();
		else if(Input.GetMouseButtonDown(2)) MiddleMouseUnClick();
		MouseHover();
	}
	
	
	private void LeftMouseClick() { 
		if(player.hud.MouseInBounds()) {
			if(player.IsFindingBuildingLocation()) {
				if(player.CanPlaceBuilding()) player.StartConstruction();
			} else {
				selectRectangle.SetStartCoordinates(Input.mousePosition);
				/*GameObject hitObject = WorkManager.FindHitObject(Input.mousePosition);
				Vector3 hitPoint = WorkManager.FindHitPoint(Input.mousePosition);
				if(hitObject && hitPoint != ResourceManager.InvalidPosition) {
					player.selectedObjects.MouseClick(hitObject, hitPoint, player);
				}*/
			}
		}
	}
	
	private void LeftMouseUnClick(){
		
		if(selectRectangle.CheckIfBigEnough()){//u got to decide, if you are selecting area, or telling the selected object to do sth
			selectRectangle.MakeSelection();
		}
		else{

			selectRectangle.draw = false;// i tak trza to wylaczyc
			/////////////////Here is sending info about click to objects!
			GameObject hitObject = WorkManager.FindHitObject(Input.mousePosition);
			Vector3 hitPoint = WorkManager.FindHitPoint(Input.mousePosition);
			if(hitObject && hitPoint != ResourceManager.InvalidPosition) {
				player.selectedObjects.MouseClick(hitObject, hitPoint, player);
				if(hitObject.name != "Ground") {
						WorldObject worldObject = hitObject.transform.parent.GetComponent<WorldObject>();//////////////This is selecting!
						if(worldObject) {
							//player.selectedObjects.AddOne(worldObject);			<== this is not needed!, everything is made in SetSelection!
							if(!Input.GetButton("ChangeSelection")){ //We are ot editing the amount of selected objects, and the user clicked on some object not while selecting other ones, so we have to clear selection
								player.selectedObjects.ClearSelection();
							}
								worldObject.SetSelection(true, player.hud.GetPlayingArea());
							TryToSimulateFire(worldObject);
						}	
				}
			}
			else{
				
			}
		}	
	}
	
	private void RightMouseClick() {
		if(player.hud.MouseInBounds() && !Input.GetKey(KeyCode.LeftAlt) && player.selectedObjects.IfAny()) {
			if(player.IsFindingBuildingLocation()) {
				player.CancelBuildingPlacement();
			} else {
				player.selectedObjects.ClearSelection();
			}
		}
	}
	
	private void RightMouseUnClick(){//Sorting according to parent class np Building or Unit

	}
	
	private void MiddleMouseUnClick(){Debug.Log("LOL");
		GameObject hitObject = WorkManager.FindHitObject(Input.mousePosition);
			Vector3 hitPoint = WorkManager.FindHitPoint(Input.mousePosition);
			if(hitObject && hitPoint != ResourceManager.InvalidPosition) {
				if(hitObject.name != "Ground") {
				MoveCameraToObject(hitObject);
				}
			}
	}
	
	private void MouseHover() {
		if(player.hud.MouseInBounds()) {			
			if(player.IsFindingBuildingLocation()) {
				player.FindBuildingLocation();
			} else {
				GameObject hoverObject = WorkManager.FindHitObject(Input.mousePosition);
				Vector3 hitPoint = WorkManager.FindHitPoint(Input.mousePosition);
				selectRectangle.SetEndCoordinates(Input.mousePosition);
				if(hoverObject) {
						WorldObject Wobj = hoverObject.transform.parent.GetComponent<WorldObject>();
						if(Wobj != null){
						
							 Wobj.SetHoverState(hoverObject);
						}
					
					else if(hoverObject.name != "Ground") {
						Player owner = hoverObject.transform.root.GetComponent<Player>();
						if(owner) {
							Unit unit = hoverObject.transform.parent.GetComponent<Unit>();
							Building building = hoverObject.transform.parent.GetComponent<Building>();
							if(owner.username == player.username && (unit || building)) player.hud.SetCursorState(CursorState.Select);
						}
					}
				}
			}
		}
	}
	
	public void MoveCameraToObject(GameObject obj){

			Vector3 ObjPosition = obj.transform.position;
			CameraTargetPosition.x = ObjPosition.x;
			CameraTargetPosition.z = ObjPosition.z;

			IsCameraMovingToObject = true;

	}
	
	public void MoveCameraToPosition(Vector3 pos){
			CameraTargetPosition.x = pos.x;
			CameraTargetPosition.z = pos.z;
			IsCameraMovingToObject = true;

	}
	
	private void KeyboardActivity(){ 
		if( Input.GetButtonDown("SelectAllBuildings")) player.selectedObjects.SelectAllBuildings();
		if(Input.GetButtonDown("SelectAllUnits")) player.selectedObjects.SelectAllUnits();
	}
	
	public void TryToSimulateFire(WorldObject worldObject){//Try to call function in building class about fire
		


		Building selectedBuilding = worldObject.GetComponent<Building>();
			if(selectedBuilding){
				selectedBuilding.FireDamage();
			}
	}
	
}

