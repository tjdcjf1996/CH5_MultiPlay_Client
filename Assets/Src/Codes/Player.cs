using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEditor.Rendering;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class Player : MonoBehaviour
{
    public Vector2 inputVec;
    public float speed;
    public string deviceId;
    public RuntimeAnimatorController[] animCon;

    Rigidbody2D rigid;
    SpriteRenderer spriter;
    Animator anim;
    TextMeshPro myText;

    void Awake()
    {
        rigid = GetComponent<Rigidbody2D>();
        spriter = GetComponent<SpriteRenderer>();
        anim = GetComponent<Animator>();
        myText = GetComponentInChildren<TextMeshPro>();
    }

    void OnEnable() {

        if (deviceId.Length > 5) {
            myText.text = deviceId[..5];
        } else {
            myText.text = deviceId;
        }
        myText.GetComponent<MeshRenderer>().sortingOrder = 6;
        
        anim.runtimeAnimatorController = animCon[GameManager.instance.playerId];
    }

    // Update is called once per frame
    private float updateInterval = 0.05f; // 100ms마다 전송
private float nextUpdateTime;

void Update()
{
    if (!GameManager.instance.isLive) {
        return;
    }

    // 이동 입력 처리
    inputVec.x = Input.GetAxisRaw("Horizontal");
    inputVec.y = Input.GetAxisRaw("Vertical");

    // 플레이어가 이동 중인지 확인
    if (inputVec.x != 0 || inputVec.y != 0) {
        if (Time.time >= nextUpdateTime) {
            // 위치 이동 패킷 전송 -> 서버로
            NetworkManager.instance.SendLocationUpdatePacket(rigid.position.x, rigid.position.y);
            nextUpdateTime = Time.time + updateInterval; // 다음 전송 시간 설정
        }
    }
}


    void FixedUpdate() {
        if (!GameManager.instance.isLive) {
            return;
        }
        // 힘을 준다.
        // rigid.AddForce(inputVec);

        // 속도 제어
        // rigid.velocity = inputVec;

        // 위치 이동
        Vector2 nextVec = inputVec * speed * Time.fixedDeltaTime;
        rigid.MovePosition(rigid.position + nextVec);
    }

    // Update가 끝난이후 적용
    void LateUpdate() {
        if (!GameManager.instance.isLive) {
            return;
        }

        anim.SetFloat("Speed", inputVec.magnitude);

        if (inputVec.x != 0) {
            spriter.flipX = inputVec.x < 0;
        }
    }

    void OnCollisionStay2D(Collision2D collision) {
        if (!GameManager.instance.isLive) {
            return;
        }
    }
}
