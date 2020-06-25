using System;
using System.Linq;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    public Transform PlayerTransform;

    private Animator _playerAnimator;

    private void Awake()
    {
        _playerAnimator = PlayerTransform.GetComponent<Animator>();
    }

    private void Update()
    {
        if (Input.GetAxis("Horizontal") > 0.01f)
        {
            _playerAnimator.speed = 1f;
            PlayerTransform.localScale = new Vector3(1f, 1f, 1f);
            PlayerTransform.localRotation = Quaternion.Euler(0f, 0f, 0f);
        }
        else if (Input.GetAxis("Horizontal") < -0.01f)
        {
            _playerAnimator.speed = 1f;
            PlayerTransform.localScale = new Vector3(-1f, 1f, 1f);
            PlayerTransform.localRotation = Quaternion.Euler(0f, 0f, 0f);
        }
        else if (Input.GetAxis("Vertical") > 0.01f)
        {
            _playerAnimator.speed = 1f;
            PlayerTransform.localScale = new Vector3(1f, 1f, 1f);
            PlayerTransform.localRotation = Quaternion.Euler(0f, 0f, 90f);
        }
        else if (Input.GetAxis("Vertical") < -0.01f)
        {
            _playerAnimator.speed = 1f;
            PlayerTransform.localScale = new Vector3(1f, 1f, 1f);
            PlayerTransform.localRotation = Quaternion.Euler(0f, 0f, 270f);
        }
        else
        {
            _playerAnimator.speed = 0f;
        }
    }
}
