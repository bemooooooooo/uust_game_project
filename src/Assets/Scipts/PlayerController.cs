using UnityEngine;
using Core;


public class PlayerController : PlayerCore
{
    private Rigidbody2D rb;

    private void Start()
    {
        rb = GetComponent<Rigidbody2D>();
    }

    private void FixedUpdate()
    {
        Move(rb);
        Jump(rb);
    }
}
