using System.Collections;
using System.Collections.Generic;
using UnityEngine;

class BasketballCollision : MonoBehaviour
{
    private BasketballCannonController cannonController;
    private bool hasHitHoop = false;

    public void SetCannonController(BasketballCannonController controller)
    {
        cannonController = controller;
    }

    void OnCollisionEnter(Collision collision)
    {
        if (cannonController == null) return;

        if (!collision.gameObject.CompareTag("Hoop") &&
            !collision.gameObject.CompareTag("ScoreNet"))
        {
            cannonController.PlaySound(cannonController.bounceSound);
        }

        if (collision.gameObject.CompareTag("Hoop"))
        {
            hasHitHoop = true;
        }
        else if (collision.gameObject.CompareTag("ScoreNet"))
        {
            int points = hasHitHoop ? cannonController.hoopPoints : cannonController.scoreNetPoints;
            cannonController.OnBallCollision(gameObject, collision.gameObject.tag, points);
        }
    }
}

