using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Data;
using UnityEngine;
using UnityEngine.EventSystems;
using static Define;
using Object = UnityEngine.Object;
using Random = UnityEngine.Random;

public class GameManager
{
    #region Base

    private GameScene _scene;
    private bool _nowGameScene = true;

    public void Clear()
    {

    }

    public void Init()
    {

    }

    public void Update()
    {
        //if (_scene == null)
        //    return;

        //if (_nowGameScene == false)
        //    return;

        // �Է� ó��
        UpdateInput();
    }

    private void UpdateInput()
    {
        if (IsPointerOverUIObject(Input.mousePosition))
            return;

        if (Input.GetMouseButtonDown(0))
        {

        }
        else if (Input.GetMouseButtonUp(0))
        {
            //Debug.Log("Touch Position: " + Input.mousePosition);

            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            RaycastHit2D hit = Physics2D.GetRayIntersection(ray);

            if (hit.collider != null)
            {
                //Debug.Log(hit.transform.name);

                if (hit.transform.TryGetComponent<ForgeController>(out var forge))
                {
                    forge.HitForge();
                }
            }
        }
    }

    public bool IsPointerOverUIObject(Vector2 touchPos)
    {
        PointerEventData eventDataCurrentPosition = new PointerEventData(EventSystem.current);
        eventDataCurrentPosition.position = touchPos;
        List<RaycastResult> results = new List<RaycastResult>();

        EventSystem.current.RaycastAll(eventDataCurrentPosition, results);

        return results.Count > 0;
    }
    #endregion

    #region GameScene

    int weaponMaxHp = 100;
    int weaponHp = 0;
    public float CalcWeaponHit()
    {
        if(weaponHp >= weaponMaxHp)
        {
            weaponHp = 0;
        }

        weaponHp += 10;

        if(weaponHp >= weaponMaxHp)
        {
            weaponHp = weaponMaxHp;
        }

        Debug.Log(weaponHp);

        return (float)weaponHp / weaponMaxHp;
    }
    #endregion
}