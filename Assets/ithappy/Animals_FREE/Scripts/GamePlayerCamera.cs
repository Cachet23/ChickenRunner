using UnityEngine;
using Controller;

public class GamePlayerCamera : PlayerCamera
{
    protected override void Awake()
    {
        base.Awake();
        
        // Initialisiere Startposition
        if (m_Player != null)
        {
            m_Target.position = m_Player.position;
        }
    }

    void LateUpdate()
    {
        if (m_Player != null)
        {
            // Update Target Position
            m_Target.position = Vector3.Lerp(m_Target.position, m_Player.position, Time.deltaTime * 5f);

            // Berechne Kamera Position
            var rotation = Quaternion.Euler(m_Angles.x, m_Angles.y, 0);
            var position = m_Target.position - (rotation * Vector3.forward * m_Distance);
            
            // Setze Kamera Position und Rotation
            m_Transform.rotation = rotation;
            m_Transform.position = position;
        }
    }
}