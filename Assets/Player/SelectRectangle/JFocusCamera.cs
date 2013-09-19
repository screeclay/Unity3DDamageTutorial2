using UnityEngine;
using System.Collections;

namespace RTS{
public class JFocusCamera : MonoBehaviour {
 
 
    Bounds CalculateBounds(GameObject go) {
        Bounds b = new Bounds(go.transform.position, Vector3.zero);
        Object[] rList = go.GetComponentsInChildren(typeof(Renderer));
        foreach (Renderer r in rList) {
            b.Encapsulate(r.bounds);
        }
        return b;
    }
 
    void FocusCameraOnGameObject(Camera c, GameObject go) {
        Bounds b = CalculateBounds(go);
        Vector3 max = b.size;
        float radius = Mathf.Max(max.x, Mathf.Max(max.y, max.z));
        float dist = radius /  (Mathf.Sin(c.fieldOfView * Mathf.Deg2Rad / 2f));
        Debug.Log("Radius = " + radius + " dist = " + dist);
        Vector3 pos = Random.onUnitSphere * dist + b.center;
        c.transform.position = pos;
        c.transform.LookAt(b.center);
    }
 
    // Update is called once per frame
    void Update () {
        if (Input.GetMouseButtonDown(0)) {
            Camera c = Camera.main;
            Vector3 mp = Input.mousePosition;
            Ray ray = c.ScreenPointToRay(mp);
            RaycastHit hit;
            if (Physics.Raycast(ray, out hit, Mathf.Infinity)) {
                FocusCameraOnGameObject(Camera.main, hit.transform.gameObject);
            }
        }
    }
 
}
	
}