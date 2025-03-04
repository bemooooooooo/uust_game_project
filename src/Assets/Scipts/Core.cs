using NUnit.Framework.Interfaces;
using UnityEngine;
using UnityEngine.Rendering.Universal;
using UnityEngine.UI;

namespace Core
{

    public enum DifficultyLevel
    {
        Easy,
        Medium,
        Hard
    }
    public class PlayerCore: MonoBehaviour
    {
        [Header("Stats Settings")]
        public float playerHealth;
        public float playerLucky;
        public float playerArmor;
        public float playerDeteriorationArmor;
        public float playerStrength;
        private readonly float strengthRatio = 0.3f;

        [Header("Jump Settings")]
        public float playerJumpForce;
        private bool isGrounded;
        public Transform groundCheck;
        public float groundCheckRadius;
        public LayerMask groundLayer;

        [Header("Move Settings")]
        public float playerMoveSpeed;
        public float playerSprintSpeed;
        public readonly float playerBuffSpeed_1;
        public readonly float playerBuffSpeed_2;
        private float moveInput;
        private bool facingRight;


        [Header("Mentality Settings")]
        public float playerMentality;
        public float PlayerMentality { get => playerMentality; set => playerMentality = value; }

        [Header("Attack Settings")]
        public float playerBaseDamage;
        public float playerAttackRange;
        public LayerMask enemyLayer;

        [Header("Spell Cast Settings")]
        public GameObject fireballPrefab;
        public float spellCooldown = 2f;
        private float lastSpellTime;

        protected void Attack()
        {
            if (Input.GetKeyDown(KeyCode.Mouse0))
            {
                Collider2D[] hitEnemies = Physics2D.OverlapCircleAll(transform.position, playerAttackRange, enemyLayer);
                foreach (Collider2D enemy in hitEnemies)
                {
                    enemy.GetComponent<EnemyCore>().TakeDamage(playerBaseDamage, playerStrength, playerLucky); // Написать класс Enemy чтобы работало
                }
                // {Анимация атаки}
            }
        }

        protected void CastSpell()
        {
            if (Time.time > lastSpellTime + spellCooldown || lastSpellTime == null)
            {
                Instantiate(fireballPrefab, transform.position, Quaternion.identity);
                lastSpellTime = Time.time;
                // {Анимация заклинания}
            }
        }

        protected void Move(Rigidbody2D rb)
        {
            moveInput = Input.GetAxis("Horizontal");
            float speed = Input.GetKey("left ctrl") ? playerSprintSpeed : playerMoveSpeed;
            rb.linearVelocity = new Vector2(moveInput*speed, rb.linearVelocityY);
            //{Анимация движения}
            FlipModel(moveInput);
        }

        protected void Jump(Rigidbody2D rb)
        {
            isGrounded = Physics2D.OverlapCircle(groundCheck.position, groundCheckRadius, groundLayer);
            if (Input.GetAxis("Jump") > 0)
            {
                if (isGrounded)
                {
                    //{Анимация прыжка}
                    rb.linearVelocity = new Vector2(rb.linearVelocityX, playerJumpForce);
                }
            }
        }

        private void FlipModel(float move)
        {
            Quaternion rotation = transform.rotation;
            if (move < 0 && facingRight)
            {
                facingRight = false;
                rotation.y = 180;
            }
            else if (move > 0 && !facingRight)
            {
                facingRight = true;
                rotation.y = 0;
            }
            transform.rotation = rotation;
        } 

        public void TakeDamage(float damage, float strength, float lucky)
        {
            playerHealth -= CalculateDamage(damage, strength, lucky);
            if (playerHealth <= 0)
            {
                Die();
            }
        }

        private void Die()
        {
            // Аниманиция смерти
            // + логика смерти
        }

        private float CalculateDamage(float damage, float strength, float lucky)
        {
            return Mathf.Min((damage + Mathf.Max(strength-playerStrength, 0) * strengthRatio + 
                lucky/100 * Random.Range(0,10)) * 
                (1 - playerArmor*(1-playerDeteriorationArmor)/100), damage + 30f);
        }
    }

    public enum EnemyType
    {
        Thief,
        Mage,
        Swordsman,
        Paladin,
        Archer
            // Изменяемый список, не забудьте после добавления
            // прописать нужные методы в EnemyCore
    }

    public class EnemyCore: MonoBehaviour
    {
        [Header("Main Settings")]
        public DifficultyLevel difficultyLevel = DifficultyLevel.Easy;
        public EnemyType enemyType; 
        private Transform player;

        [Header("Stats Settings")]
        public float enemyHealth;
        public float enemyArmor;
        public float enemyDeteriorationArmor;
        public float enemyLucky;
        public float enemyStrength;
        private readonly float strengthRatio = 0.4f;

        [Header("Move Settings")]
        public float enemyMoveSpeed;
        public float enemyDetectionRange;

        [Header("Attack Settings")]
        public float enemyAttackRange;
        public float enemyDodgeChance;
        public float enemyParryChance;
        public float enemyBlockChance;
        public float enemyRretreatHealthThreshold = 30f;
        public float enemyMagicAttackRange = 7f; // Для мага и лучника
        public float enemyHealAmount = 20f; // Для паладина

        private bool facingRight = true;
        private bool isRetreating = false;

        public void Start()
        {
            player = GameObject.FindGameObjectWithTag("Player").transform;
            ApplyDifficulty();
        }

        private void ApplyDifficulty()
        {
            switch (difficultyLevel)
            {
                case DifficultyLevel.Easy:
                    enemyMoveSpeed *= 0.8f;
                    enemyDodgeChance *= 0.5f;
                    enemyParryChance *= 0.5f;
                    enemyBlockChance *= 0.5f;
                    break;
                case DifficultyLevel.Medium:
                    enemyMoveSpeed *= 1f;
                    enemyDodgeChance *= 1f;
                    enemyParryChance *= 1f;
                    enemyBlockChance *= 1f;
                    break;
                case DifficultyLevel.Hard:
                    enemyMoveSpeed *= 1.5f;
                    enemyDodgeChance *= 1.5f;
                    enemyParryChance *= 1.5f;
                    enemyBlockChance *= 1.5f;
                    break;
            }
        }

        public void Update()
        {
            if (enemyHealth <= 0) return;
            if (enemyHealth < enemyRretreatHealthThreshold && !isRetreating)
            {
                StartRetreat();
            }
            if (isRetreating)
            {
                Retreat();
                return;
            }
            switch (enemyType)
            {
                case EnemyType.Thief:
                    ThiefBehavior();
                    break;
                case EnemyType.Archer:
                    ArcherBehavior();
                    break;
                case EnemyType.Swordsman:
                    SwordsmanBehavior();
                    break;
                case EnemyType.Paladin:
                    PaladinBehavior();
                    break;
                case EnemyType.Mage:
                    MageBehavior();
                    break;
            }
        }

        private void StartRetreat()
        {
            // Анимация разворота и бега в страхе 
        }

        private void Retreat()
        {
            // Бег в страхе от игрока
        }

        private void ThiefBehavior()
        {
            // Поведение вора
        }

        private void ArcherBehavior()
        {
            // Поведение лучника
        }

        private void SwordsmanBehavior()
        {
            // Поведение мечника
        }

        private void PaladinBehavior()
        {
            // Поведение паладина
        }

        private void MageBehavior()
        {
            // Поведение мага
        }

        public void TakeDamage(float damage, float strength, float lucky)
        {
            if(Random.value < enemyDodgeChance)
            {
                // Анимация уклонения
                return;
            }
            if(Random.value < enemyParryChance)
            {
                // Анимация парирования
                player.GetComponent<PlayerCore>().TakeDamage(damage*0.7f, enemyStrength, enemyLucky);
                return;
            }
            if(Random.value < enemyBlockChance)
            {
                // Анимация блокирования удара
                damage *= 0.5f;
                return;
            }
            enemyHealth -= CalculateDamage(damage, strength, lucky);
            if(enemyHealth <= 0)
            {
                Die();
            }
        }

        private void Die()
        {
            // Анимация смерти
            Destroy(gameObject);
        }
        private float CalculateDamage(float damage, float strength, float lucky)
        {
            return Mathf.Min((damage + Mathf.Max(strength - enemyStrength, 0) * strengthRatio +
                lucky / 100 * Random.Range(0, 10)) *
                (1 - enemyArmor * (1 - enemyDeteriorationArmor) / 100), damage + 30f);
        }
    }
}