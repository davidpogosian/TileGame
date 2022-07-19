using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;
using System;
using UnityEngine.UI;

public class Building : NetworkBehaviour
{
    BuildingState state;
    GameObject target;
    bool reloading = false;
    GameObject myBullet;

    Vector3 ogBulletPos;
    Vector3 loc;
    Slider reloadSlider; // building UI is here
    NetworkVariable<float> reloadIntrpl = new NetworkVariable<float>(1); 

    private void Start()
    {
        
        switch (gameObject.GetComponent<NetworkObject>().OwnerClientId)
        {
            case 0:
                transform.Find("Box").GetComponent<Renderer>().material.color = Color.blue;
                break;
            case 1:
                transform.Find("Box").GetComponent<Renderer>().material.color = Color.red;
                break;
            case 2:
                transform.Find("Box").GetComponent<Renderer>().material.color = Color.yellow;
                break;
            case 3:
                transform.Find("Box").GetComponent<Renderer>().material.color = Color.green;
                break;
        }

        state = BuildingState.idle;
        reloadSlider = transform.Find("Canvas").transform.Find("ReloadSlider").GetComponent<Slider>();
        myBullet = transform.Find("Pom(Clone)").gameObject;        
    }

    enum BuildingState
    {
        idle,
        lockedIn,
    }

    private void Update()
    {
        reloadSlider.value = reloadIntrpl.Value;

        if (!IsServer) { return; }
        switch(state) // this is stupid
        {
            case BuildingState.idle:
                Collider[] inRange = Physics.OverlapSphere(transform.position, 30, 64);
                if (inRange.Length > 0)
                {
                    target = inRange[0].transform.parent.gameObject;
                    state = BuildingState.lockedIn;
                }
                break;
            case BuildingState.lockedIn:
                
                if (target == null) { state = BuildingState.idle; return; }
                if (reloading) { return; }

                StartCoroutine(Reload());
                state = BuildingState.idle;                
                reloading = true;
                
                StartCoroutine(Fire());
                break;
        }
    }
    IEnumerator Reload()
    {
        reloadIntrpl.Value = 0;
        while (reloadIntrpl.Value < 1)
        {            
            yield return new WaitForSeconds(0.1f);
            reloadIntrpl.Value += 0.02f;
            reloadIntrpl.Value = Mathf.Clamp(reloadIntrpl.Value, 0, 1);
        }
        
        reloading = false;
    }

    IEnumerator Fire()
    {
        reloadSlider.value = 0;
        ogBulletPos = myBullet.transform.position;
        loc = target.transform.position;

        // c = sqrt(a^2 + b^2)
        float dist = Mathf.Pow(Mathf.Pow(loc.x - ogBulletPos.x, 2) + Mathf.Pow(loc.z - ogBulletPos.z, 2), 0.5f);

        Vector4 row1 = new Vector4(1, 0, 0, myBullet.transform.position.y); // initial
        Vector4 row2 = new Vector4(1, dist / 2, Mathf.Pow(dist / 2, 2), 10); //mid
        Vector4 row3 = new Vector4(1, dist, Mathf.Pow(dist, 2), target.transform.position.y); // end

        // row reduce 

        row2 = row2 - row1;
        row3 = row3 - row1;

        row3 = row3 - 2 * row2;

        row3 = (1 / row3[2]) * row3;

        row2 = row2 - (row2[2] * row3);

        row2 = (1 / row2[1]) * row2;

        float c = row1[3];
        float b = row2[3];
        float a = row3[3];

        float traveled;
        float intrpl1 = 0;
        float intrpl2 = 0;
        float x = Mathf.Lerp(ogBulletPos.x, loc.x, intrpl1);
        float z = Mathf.Lerp(ogBulletPos.z, loc.z, intrpl2);

        while (intrpl1 != 1)
        {
            intrpl1 += 0.8f * Time.deltaTime;
            intrpl2 += 0.8f * Time.deltaTime;
            intrpl1 = Mathf.Clamp(intrpl1, 0, 1);
            intrpl2 = Mathf.Clamp(intrpl1, 0, 1);
            x = Mathf.Lerp(ogBulletPos.x, loc.x, intrpl1);
            z = Mathf.Lerp(ogBulletPos.z, loc.z, intrpl2);

            Vector3 bul = myBullet.transform.position;
            traveled = dist - Mathf.Pow(Mathf.Pow(loc.x - bul.x, 2) + Mathf.Pow(loc.z - bul.z, 2), 0.5f);
            traveled = Mathf.Clamp(traveled, 0, dist);
            float altitude = a * Mathf.Pow(traveled, 2) + b * traveled + c;

            myBullet.transform.position = (new Vector3(x, altitude, z));
            yield return null;
        }

        Collider[] hitEnemies = Physics.OverlapSphere(myBullet.transform.position, 5f, 64);
        foreach (Collider enemy in hitEnemies)
        {
            enemy.transform.parent.gameObject.GetComponent<SquigBehaviour>().TakeDamage(34, gameObject.GetComponent<NetworkObject>().OwnerClientId);
        }
        myBullet.SetActive(false);
        myBullet.transform.position = ogBulletPos;
        myBullet.SetActive(true);
    }
}
