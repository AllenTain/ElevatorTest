using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Runner : MonoBehaviour
{
    public float runSpeed;
    public enum RunDir { left, right, none };
    public RunDir runDir;
    public bool falling;
    Animator animator;
    Rigidbody2D body;
    // Start is called before the first frame update
    void Start()
    {
        body = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>();
    }

    // Update is called once per frame
    void Update()
    {
        switch (runDir)
        {
            case RunDir.left:
                body.velocity = Vector2.left * runSpeed;
                break;
            case RunDir.right:
                body.velocity = Vector2.right * runSpeed;
                break;
            case RunDir.none:
                body.velocity = Vector2.right * 0f;
                break;
        }
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (falling)
            animator.SetTrigger("explode");
    }
}
