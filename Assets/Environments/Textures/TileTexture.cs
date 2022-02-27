using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[ExecuteInEditMode]
public class TileTexture : MonoBehaviour
{
    public float UnitScaleY = 1.75f;

    void Start()
    {
        gameObject.GetComponent<Renderer>().sharedMaterial.mainTextureScale = new Vector2(1f, gameObject.transform.localScale.y / UnitScaleY);
    }
}
