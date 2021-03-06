﻿using UnityEngine;
using UnityEngine.UI;

public class Car : MonoBehaviour
{
    public const float MaxFuel = 80f;
    public const int MaxHealth = 100;

    [SerializeField] private float _speed = 1000f;
    [SerializeField] private float _rotationSpeed = 80f;
    [SerializeField] private Transform[] _wheels;
    [SerializeField] private Transform _saw;
    [SerializeField] private Transform _saw1;
    [SerializeField] private Transform _saw2;
    [SerializeField] private Text _tutorial;
    
    private Rigidbody _rigidbody;
    private AudioSource _audioSource;
    private Vector3 _velocity;
    private float _fuel = MaxFuel;
    private float _power;
    private float _sawTimer;
    private float _sawMaxTime = 5.5f;
    private bool _enableSaw;
    private float _nitroTimer;
    private float _nitroMaxTime = 5.5f;
    private bool _enableNitro;
    private float _originalSpeed;
    private int _health = MaxHealth;
    private bool _moved;

    private void Awake()
    {
        _rigidbody = GetComponent<Rigidbody>();
        _audioSource = GetComponent<AudioSource>();
    }

    private void Update()
    {
        if (_moved)
        {
            _tutorial.color = Color.Lerp(_tutorial.color, new Color(1, 1, 1, 0), 5 * Time.deltaTime);
        }
        
        if (ZombieManager.Instance.IsGameOver())
        {
            _power = 0;
            _velocity = Vector3.zero;
            return;
        }

        if (_fuel <= 0f || _health <= 0f)
        {
            _audioSource.Pause();
            _power = 0f;
            _velocity = Vector3.zero;
            ZombieManager.Instance.GameOver();
            return;
        }
    
        HandleSaw();
        HandleNitro();

        _power = Input.GetAxis("Vertical") * _speed;
        _velocity = transform.forward * _power;

        if (_velocity.magnitude <= 0f)
        {
            _audioSource.Pause();
            return;
        }

        _moved = true;
        _fuel -= Time.deltaTime;
        RotatePlayer();

        _audioSource.UnPause();
    }

    private void HandleSaw()
    {
        if (!_enableSaw)
        {
            _saw1.transform.localPosition = Vector3.Lerp(
                _saw1.transform.localPosition,
                new Vector3(.8f, 0, 0),
                5f * Time.deltaTime
            );

            _saw2.transform.localPosition = Vector3.Lerp(
                _saw2.transform.localPosition,
                new Vector3(-.8f, 0, 0),
                5f * Time.deltaTime
            );

            return;
        }

        _saw1.transform.localPosition = Vector3.Lerp(
            _saw1.transform.localPosition,
            new Vector3(0, 0, 0),
            5f * Time.deltaTime
        );

        _saw2.transform.localPosition = Vector3.Lerp(
            _saw2.transform.localPosition,
            new Vector3(0, 0, 0),
            5f * Time.deltaTime
        );

        _sawTimer -= Time.deltaTime;
        if (_sawTimer <= 0)
        {
            _enableSaw = false;
            _saw.GetComponent<AudioSource>().Stop();
        }
    }

    private void HandleNitro()
    {
        if (!_enableNitro)
        {
            _originalSpeed = _speed;
            return;
        }

        _speed = _originalSpeed * 2.5f;

        _nitroTimer -= Time.deltaTime;
        if (_nitroTimer <= 0)
        {
            _speed = _originalSpeed;
            _enableNitro = false;
        }
    }

    private void RotatePlayer()
    {
        var f = _power >= 0 ? 1 : -1;
        transform.Rotate(0, Input.GetAxis("Horizontal") * _rotationSpeed * Time.deltaTime * f, 0);

        foreach (var wheel in _wheels)
        {
            wheel.transform.Rotate(new Vector3(1, 0, 0), _velocity.normalized.magnitude * 10);
        }
    }

    private void FixedUpdate()
    {
        _rigidbody.velocity = new Vector3(
            _velocity.x * Time.fixedDeltaTime,
            _rigidbody.velocity.y,
            _velocity.z * Time.fixedDeltaTime
        );
    }

    public float GetFuel() => _fuel;
    public int GetHealth() => _health;
    public float GetPower() => _power;

    public void Refuel(int amount = 16)
    {
        _fuel += amount;
        _fuel = Mathf.Clamp(_fuel, 0, MaxFuel);
    }

    public void AddSaw()
    {
        _saw.gameObject.SetActive(true);
    }

    public void StartSaw()
    {
        _enableSaw = true;
        _sawTimer = _sawMaxTime;
        _saw.GetComponent<AudioSource>().Play();
    }
    
    public void ImproveSaw()
    {
        _sawMaxTime += 2f;
    }

    public void Nitro()
    {
        _enableNitro = true;
        _nitroTimer = _nitroMaxTime;
    }

    public void ImproveNitro()
    {
        _nitroMaxTime += 2f;
    }
    
    public void Damage()
    {
        _health -= 2;
        _health = Mathf.Clamp(_health, 0, MaxHealth);
    }

    public void Heal()
    {
        _health += 15;
        _health = Mathf.Clamp(_health, 0, MaxHealth);
    }
}