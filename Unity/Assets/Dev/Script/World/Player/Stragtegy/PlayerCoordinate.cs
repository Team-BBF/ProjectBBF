using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class PlayerCoordinate : MonoBehaviour, IPlayerStrategy
{
    private PlayerController _controller; 
    
    public void Init(PlayerController controller)
    {
        _controller = controller;
    }

    public void OnUpdate()
    {
    }

    public Vector3 GetFront()
    {
        var worldPos = _controller.transform.position;
        Vector3 dir = Vector3.zero;

        if (Mathf.Approximately(_controller.MoveStrategy.LastMovedDirection.y, 0f) == false)
        {
            dir = new Vector3(
                _controller.MoveStrategy.LastMovedDirection.x + _controller.InteractionDirFactor.x ,
                _controller.MoveStrategy.LastMovedDirection.y * _controller.InteractionDirFactor.y,
                0f);
        }
        else
        {
            dir = new Vector3(
                _controller.MoveStrategy.LastMovedDirection.x * _controller.InteractionOffset.x ,
                _controller.MoveStrategy.LastMovedDirection.y + _controller.InteractionOffset.y,
                0f);
        }

        return worldPos + dir;
    }

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
        Gizmos.DrawWireSphere(GetFront(), _controller.InteractionRadius);
    }
}
