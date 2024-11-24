using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class PlayerCoordinate : MonoBehaviour, IPlayerStrategy
{
    private PlayerController _controller;
    private PlayerCoordinateData _data;
    
    public void Init(PlayerController controller)
    {
        _controller = controller;
        _data = _controller.CoordinateData;
    }

    public Vector3 GetFront()
    {
        var worldPos = _controller.transform.position;
        Vector3 dir = GetFrontDir();

        return worldPos + dir;
    }

    public Vector2 GetDirOffset(Vector2 direction)
    {
        Vector3 dir = Vector3.zero;

        // 위 아래를 바라보고 있을 떄
        if (Mathf.Approximately(direction.y, 0f) == false)
        {
            float lookDir = Mathf.Sign(direction.y);
            
            if (lookDir > 0f) // 위
            {
                dir = _data.UpOffset;
            }
            else // 아래
            {
                dir = _data.DownOffset;
            }
            
        }
        else
        {
            float lookDir = Mathf.Sign(direction.x);
            dir = new Vector3(lookDir * _data.SideOffset.x, _data.SideOffset.y);
        }

        return dir;
    }

    public Vector3 GetFrontDir() => GetDirOffset(_controller.MoveStrategy.LastMovedDirection);
    public Vector3 GetFrontPureDir()
    {
        Vector3 dir = new Vector3
            (
                _controller.MoveStrategy.LastMovedDirection.x,
                _controller.MoveStrategy.LastMovedDirection.y,
                0f
            );
        
        return dir.normalized;
    }

    public Vector2 GetLookAtDir()
    {
        Vector2 clickPoint = Camera.main.ScreenToWorldPoint(InputManager.Map.Player.Look.ReadValue<Vector2>());
        Vector2 dir = clickPoint - (Vector2)_controller.transform.position;

        float dis = dir.magnitude;
        dir.Normalize();
        
        float minDis = Mathf.Min(GetDirOffset(_controller.VisualStrategy.ToNormalizedVector(dir, true)).magnitude, dis); 
        minDis = Mathf.Min(_data.MaxDistance, minDis);
        
        return dir.normalized * minDis;
    }
    public Vector2 GetLookAtPosition()
    { 
        return (Vector2)_controller.transform.position + GetLookAtDir();
    }
    
    private Vector2 RotateVector(Vector2 vector, float angleRadians)
    {
        // 회전된 벡터 계산
        float cosTheta = Mathf.Cos(angleRadians);
        float sinTheta = Mathf.Sin(angleRadians);

        float x = vector.x * cosTheta - vector.y * sinTheta;
        float y = vector.x * sinTheta + vector.y * cosTheta;

        return new Vector2(x, y);
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(GetLookAtPosition(), _data.Radius);
    }
}
