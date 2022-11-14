﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    public float maxSpeed = 5f;
    public float jumpPower = 20f;
    public float bouncPower = 10f;
    public float invulnTIme = 0.8f;

    bool isHit = false;

    Vector2 boxCastSize = new Vector2(0.6f, 0.05f);
    float boxCastMaxDistance = 1.0f;

    Rigidbody2D rigid;
    Animator anim;
    SpriteRenderer spriteRenderer;

    void Awake()
    {
        rigid = GetComponent<Rigidbody2D>();
        anim = GetComponent<Animator>();
        spriteRenderer = GetComponentInChildren<SpriteRenderer>(); // 하위 오브젝트
    }

    void Update()
    {
        if (!isHit)
        {
            // 점프 (점프를 누르고, 점프 중이 아니고, 피격 상태가 아니면)
            if (Input.GetButtonDown("Jump") && !anim.GetBool("isJumping") && !IsInvoking("reRotate"))
            {
                //rigid.velocity = new Vector2(rigid.velocity.x, 0); // 점프 속도 초기화
                rigid.AddForce(Vector2.up * jumpPower, ForceMode2D.Impulse);
                anim.SetBool("isJumping", true);
            }

            // 움직임 멈춤
            if (Input.GetButtonUp("Horizontal") || Input.GetAxisRaw("Horizontal") == 0) // 바로 멈춤
                rigid.velocity = new Vector2(rigid.velocity.normalized.x * 0.0000001f, rigid.velocity.y);

            // 방향 전환
            //if (Input.GetButton("Horizontal"))
            //    spriteRenderer.flipX = Input.GetAxisRaw("Horizontal") == -1;
            if (Input.GetButton("Horizontal") && Input.GetAxisRaw("Horizontal") != 0) // 좌우키 동시 입력 버그 방지
                transform.localScale = new Vector3(Input.GetAxisRaw("Horizontal"), transform.localScale.y, transform.localScale.z);

            // 미끄러짐 방지
            if (!(Input.GetButton("Horizontal")))
                rigid.velocity = new Vector2(0, rigid.velocity.y);

            // 공격 (현재 실행 중인 애니메이션이 공격이 아니면)
            if (Input.GetButtonDown("Fire1") && !anim.GetCurrentAnimatorStateInfo(0).IsName("Player1_AttackFront"))
                Attack();
        }

        // 달리기 애니메이션
        if (Mathf.Abs(rigid.velocity.x) < 0.3) // 절댓값x가 크면 달리기
            anim.SetBool("isRunning", false);
        else
            anim.SetBool("isRunning", true);
    }

    void FixedUpdate()
    {
        if (!isHit)
        {
            // 움직임 조작
            float h = Input.GetAxisRaw("Horizontal"); // 횡으로 키를 누르면
            rigid.AddForce(Vector2.right * h, ForceMode2D.Impulse); // 이동한다
        }

        // 최고 속도
        if (rigid.velocity.x > maxSpeed) // 오른족
            rigid.velocity = new Vector2(maxSpeed, rigid.velocity.y);
        else if (rigid.velocity.x < maxSpeed * (-1)) // 왼쪽
            rigid.velocity = new Vector2(maxSpeed * (-1), rigid.velocity.y);

        // 바닥 판정
        if (rigid.velocity.y < 0)
        {
            anim.SetBool("isFalling", true); // 점프 없이 낙하

            //Debug.DrawRay(rigid.position, Vector3.down*2, new Color(0, 1, 0)); // 레이 시각화
            //RaycastHit2D rayHit = Physics2D.Raycast(rigid.position, Vector3.down, 1, LayerMask.GetMask("Map"));
            RaycastHit2D rayHit = Physics2D.BoxCast(transform.position, boxCastSize, 0f, Vector2.down, boxCastMaxDistance, LayerMask.GetMask("Map"));
            if (rayHit.collider != null) // 바닥 감지를 위해서 레이저
            {
                if (rayHit.distance < 0.9f)
                {
                    anim.SetBool("isJumping", false);
                    anim.SetBool("isFalling", false);
                }
            }
        }
    }

    //void OnDrawGizmos() // 사각 레이 기즈모
    //{
    //    RaycastHit2D raycastHit = Physics2D.BoxCast(transform.position, boxCastSize, 0f, Vector2.down, boxCastMaxDistance, LayerMask.GetMask("Map"));

    //    Gizmos.color = Color.red;
    //    if (raycastHit.collider != null)
    //    {
    //        Gizmos.DrawRay(transform.position, Vector2.down * raycastHit.distance);
    //        Gizmos.DrawWireCube(transform.position + Vector3.down * raycastHit.distance, boxCastSize);
    //    }
    //    else
    //    {
    //        Gizmos.DrawRay(transform.position, Vector2.down * boxCastMaxDistance);
    //    }
    //}

    void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.tag == "Enemy")
            onHit(collision.transform.position); // Enemy의 위치 정보 매개변수
        if (collision.gameObject.tag == "Trap")
            onHit(collision.transform.position);
    }

    void onHit(Vector2 targetPos)
    {
        gameObject.layer = 9; // 플레이어의 Layer 변경 (Super Armor)
        spriteRenderer.color = new Color(1, 1, 1, 0.4f); // 피격당했을 때 색 변경
        isHit = true;

        int dir = transform.position.x - targetPos.x > 0 ? 1 : -1; // 피격시 튕겨나가는 방향 결정
        rigid.AddForce(new Vector2(dir, 1) * bouncPower, ForceMode2D.Impulse); // 튕겨나가기

        this.transform.Rotate(0, 0, dir * (-10)); // 회전
        Invoke("reRotate", 0.4f);

        anim.SetTrigger("doHit"); // 애니메이션 트리거
        Invoke("offHit", invulnTIme); // invulnTIme 후 무적 시간 끝
    }

    void offHit()
    {
        gameObject.layer = 8; // Layer 변경
        spriteRenderer.color = new Color(1, 1, 1, 1f); // 색 변경
    }

    void reRotate() // 회전 초기화, 다시 조작 가능
    {
        this.transform.rotation = Quaternion.Euler(0, 0, 0);
        isHit = false;
    }

    void Attack()
    {
        anim.SetTrigger("doAttack");
    }
}
