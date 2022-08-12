using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MousePointerControl : MonoBehaviour
{
    [SerializeField] Texture2D cursor;
    private void Awake()
    {
        Cursor.SetCursor(cursor, Vector3.zero, CursorMode.ForceSoftware);
    }
}
