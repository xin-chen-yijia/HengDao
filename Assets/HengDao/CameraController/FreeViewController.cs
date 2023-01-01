using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FreeViewController : MonoBehaviour
{
    public bool destroyOnLoad = false;
    Transform trans_;

    [Header("平移")]
    public float moveSpeed = 5.0f;

    [Header("旋转")]
    public float xRotSpeed = 20.0f;
    public float yRotSpeed = 20.0f;

    private float viewCenterDis_ = 10.0f;
    private Transform parentTrans_ = null;

    // Start is called before the first frame update
    void Start()
    {
        trans_ = transform;
        if(!parentTrans_)
        {
            GameObject empty = new GameObject("CameraParent");
            parentTrans_ = empty.transform;

            Quaternion tmpRot = trans_.rotation;
            trans_.rotation = Quaternion.identity;
            parentTrans_.position = trans_.position + trans_.forward * viewCenterDis_;
            trans_.SetParent(parentTrans_);
            empty.transform.rotation = tmpRot;

            if(destroyOnLoad)
            {
                DontDestroyOnLoad(parentTrans_);
            }
        }
    }

    // Update is called once per frame
    void Update()
    {
        float hx = 0.0f;        
        float hy = 0.0f;
        float hz = 0.0f;
        hx += Input.GetAxis("Horizontal");
        hz += Input.GetAxis("Vertical");

        float dx = Input.GetAxis("Mouse X");
        float dy = Input.GetAxis("Mouse Y");

        float tSpeed = moveSpeed * Time.deltaTime;
        if (Input.GetMouseButton(0))
        {
            hx += -dx;
            hy += -dy;
            //parentTrans_.Translate((-hx+dx) * tSpeed, (-hy) * tSpeed, dy * tSpeed, Space.Self);
        }

        parentTrans_.Translate(hx * tSpeed, hy * tSpeed, hz * tSpeed, Space.Self);

        if (Input.GetKey(KeyCode.Q))
        {
            parentTrans_.Translate(0.0f, -Time.deltaTime * moveSpeed, 0.0f, Space.Self);
        }

        if (Input.GetKey(KeyCode.E))
        {
            parentTrans_.Translate(0.0f, Time.deltaTime * moveSpeed, 0.0f, Space.Self);
        }

        if (Input.GetMouseButton(1))
        {
            float xAngle = xRotSpeed * dx * Time.deltaTime;
            float yAngle = -yRotSpeed * dy * Time.deltaTime;
            //Vector3 v = Quaternion.Euler(yAngle, xAngle, 0) * trans_.forward * viewCenterDis_;
            //trans_.position = trans_.position + v - trans_.forward * viewCenterDis_;
            //transform.rotation = transform.rotation * Quaternion.Euler(yAngle, -xAngle, 0);

            xAngle = Mathf.Clamp(xAngle, -10, 30);
            yAngle = Mathf.Clamp(yAngle, -10, 10);

            parentTrans_.rotation = Quaternion.Euler(0, xAngle, 0) * parentTrans_.rotation;
            parentTrans_.Rotate(yAngle, 0, 0, Space.Self);
        }
    }
}
