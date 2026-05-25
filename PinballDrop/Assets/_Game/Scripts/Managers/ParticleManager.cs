using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ParticleManager : MonoSingleton<ParticleManager>
{
    [SerializeField] private GameObject _moneyParticle;
    [SerializeField] private GameObject _gateParticle;
    [SerializeField] private ParticleSystem _bumperHitParticle;

    public void MoneyParticle(Vector3 spawnPos)
    {
        Instantiate(_moneyParticle, spawnPos, Quaternion.identity);
    }

    public void InstantiateParticle(ParticleSystem particleSystem, Vector3 spawnPos, Quaternion rotation,
        Transform parent = null)
    {
        Instantiate(particleSystem.gameObject, spawnPos, rotation, parent);
    }

    public void GateParticle(Vector3 spawnPos)
    {
        Instantiate(_gateParticle, spawnPos, Quaternion.identity);
    }

    public void BumperBallHitParticle(Vector3 spawnPos, ColorTypes colorTypes)
    {
        var particle = Instantiate(_bumperHitParticle, spawnPos + Vector3.back, Quaternion.Euler(-90, 0, 0));
        var allParticle = particle.GetComponentsInChildren<ParticleSystem>();
        
        for (var i = 0; i < allParticle.Length; i++)
        {
            var main = allParticle[i].main;
            main.startColor = LevelManager.Instance.ObjectColors[(int)colorTypes] * .76f;
        }
       
    }
}