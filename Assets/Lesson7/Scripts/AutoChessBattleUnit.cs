using UnityEngine;

namespace AlgoLesson7
{
    public sealed class AutoChessBattleUnit : MonoBehaviour
    {
        private const float MoveSpeed = 5.5f;
        private const float BaseAttackInterval = 1.1f;

        private AutoChessArenaGame game;
        private UnitDefinition definition;
        private Transform healthFill;
        private Transform shieldRing;
        private Transform teamRing;
        private float maxHealth;
        private float health;
        private float attack;
        private float armor;
        private float shield;
        private float attackInterval;
        private float attackTimer;
        private int team;
        private int id;
        private AutoChessBoardCell currentCell;
        private AutoChessBoardCell destinationCell;

        public int Id => id;
        public int Team => team;
        public bool IsAlive => health > 0f;
        public UnitRace Race => definition.Race;
        public UnitJob Job => definition.Job;
        public float CurrentHealth => health;
        public float AttackRange => definition.AttackRange;
        public AutoChessBoardCell CurrentCell => currentCell;

        public void Initialize(
            AutoChessArenaGame owner,
            UnitDefinition data,
            int unitId,
            int unitTeam)
        {
            game = owner;
            definition = data;
            id = unitId;
            team = unitTeam;
            name = $"Unit_{unitId:00}_{data.Name}";

            CreateVisuals();
            ResetStats();
        }

        public void PlaceOnCell(AutoChessBoardCell cell, bool snapImmediately)
        {
            currentCell = cell;
            destinationCell = snapImmediately ? null : cell;

            if (snapImmediately)
                transform.position = CellStandingPosition(cell);
        }

        public void ResetStats()
        {
            maxHealth = definition.Health;
            health = maxHealth;
            attack = definition.Attack;
            armor = 0f;
            shield = 0f;
            attackInterval = BaseAttackInterval;
            attackTimer = Random.Range(0f, 0.25f);
            UpdateVisuals();
        }

        public void ApplySynergy(SynergyResult result)
        {
            ResetStats();

            if (definition.Job == UnitJob.Warrior)
                armor += result.WarriorArmor;

            if (definition.Job == UnitJob.Ranger)
                attackInterval /= Mathf.Max(0.1f, result.RangerAttackSpeedMultiplier);

            if (definition.Job == UnitJob.Mage)
                attack *= result.MageAttackMultiplier;

            if (definition.Race == UnitRace.Human)
            {
                maxHealth *= result.HumanHealthMultiplier;
                health = maxHealth;
            }

            if (definition.Race == UnitRace.Beast)
                attack *= result.BeastAttackMultiplier;

            if (definition.Race == UnitRace.Machine)
                shield = result.MachineShield;

            UpdateVisuals();
        }

        public UnitSnapshot CreateSnapshot()
        {
            return new UnitSnapshot(
                id,
                definition.Name,
                definition.Race,
                definition.Job,
                transform.position,
                health,
                definition.AttackRange,
                IsAlive);
        }

        public void TickBattle(float deltaTime)
        {
            if (!IsAlive)
                return;

            if (destinationCell != null)
            {
                Vector3 destination = CellStandingPosition(destinationCell);
                transform.position = Vector3.MoveTowards(transform.position, destination, MoveSpeed * deltaTime);

                if (Vector3.SqrMagnitude(transform.position - destination) < 0.0001f)
                    destinationCell = null;

                return;
            }

            attackTimer -= deltaTime;
            AutoChessBattleUnit target = game.FindTargetFor(this);

            if (target == null)
                return;

            int hexDistance = game.GetHexDistance(currentCell, target.CurrentCell);
            int attackRangeInHexes = definition.AttackRange <= 1.5f ? 1 : 3;

            if (hexDistance > attackRangeInHexes)
            {
                game.TryMoveOneHex(this, target);
                return;
            }

            if (attackTimer > 0f)
                return;

            attackTimer = attackInterval;
            target.ReceiveDamage(attack);
            game.SpawnAttackFlash(transform.position, target.transform.position, definition.Color);
        }

        private void ReceiveDamage(float amount)
        {
            float remaining = amount;

            if (shield > 0f)
            {
                float absorbed = Mathf.Min(shield, remaining);
                shield -= absorbed;
                remaining -= absorbed;
            }

            health -= Mathf.Max(1f, remaining - armor);

            if (health <= 0f)
            {
                health = 0f;
                game.ReleaseCell(this);
                gameObject.SetActive(false);
            }

            UpdateVisuals();
        }

        private void CreateVisuals()
        {
            Renderer renderer = GetComponent<Renderer>();
            renderer.material = CreateMaterial(definition.Color);

            transform.localScale = definition.Job switch
            {
                UnitJob.Warrior => new Vector3(1.05f, 1.25f, 1.05f),
                UnitJob.Ranger => new Vector3(0.85f, 1.05f, 0.85f),
                _ => new Vector3(0.9f, 1.15f, 0.9f)
            };

            GameObject barBack = GameObject.CreatePrimitive(PrimitiveType.Cube);
            barBack.name = "HealthBar_Back";
            barBack.transform.SetParent(transform, false);
            barBack.transform.localPosition = new Vector3(0f, 1.35f, 0f);
            barBack.transform.localScale = new Vector3(1.05f, 0.11f, 0.12f);
            barBack.GetComponent<Renderer>().material = CreateMaterial(new Color(0.08f, 0.08f, 0.08f));
            Destroy(barBack.GetComponent<Collider>());

            GameObject fill = GameObject.CreatePrimitive(PrimitiveType.Cube);
            fill.name = "HealthBar_Fill";
            fill.transform.SetParent(barBack.transform, false);
            fill.transform.localPosition = new Vector3(0f, 0f, -0.06f);
            fill.transform.localScale = new Vector3(0.94f, 0.64f, 0.55f);
            fill.GetComponent<Renderer>().material = CreateMaterial(
                team == 0 ? new Color(0.2f, 0.95f, 0.45f) : new Color(1f, 0.25f, 0.25f));
            Destroy(fill.GetComponent<Collider>());
            healthFill = fill.transform;

            GameObject ring = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            ring.name = "Shield_Ring";
            ring.transform.SetParent(transform, false);
            ring.transform.localPosition = new Vector3(0f, -0.58f, 0f);
            ring.transform.localScale = new Vector3(0.75f, 0.025f, 0.75f);
            ring.GetComponent<Renderer>().material = CreateMaterial(new Color(0.25f, 0.8f, 1f));
            Destroy(ring.GetComponent<Collider>());
            shieldRing = ring.transform;

            GameObject teamMarker = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            teamMarker.name = team == 0 ? "ALLY_Blue_Ring" : "ENEMY_Red_Ring";
            teamMarker.transform.SetParent(transform, false);
            teamMarker.transform.localPosition = new Vector3(0f, -0.62f, 0f);
            teamMarker.transform.localScale = new Vector3(0.92f, 0.035f, 0.92f);
            teamMarker.GetComponent<Renderer>().material = CreateMaterial(
                team == 0 ? new Color(0.1f, 0.55f, 1f) : new Color(1f, 0.12f, 0.12f));
            Destroy(teamMarker.GetComponent<Collider>());
            teamRing = teamMarker.transform;

            GameObject labelObject = new GameObject("World_Label");
            labelObject.transform.SetParent(transform, false);
            labelObject.transform.localPosition = new Vector3(0f, 1.57f, 0f);
            TextMesh label = labelObject.AddComponent<TextMesh>();
            label.text = team == 0 ? $"ALLY  {definition.Name}" : $"ENEMY  {definition.Name}";
            label.characterSize = 0.085f;
            label.fontSize = 34;
            label.anchor = TextAnchor.MiddleCenter;
            label.alignment = TextAlignment.Center;
            label.color = team == 0 ? new Color(0.5f, 0.82f, 1f) : new Color(1f, 0.48f, 0.48f);
        }

        private void LateUpdate()
        {
            Transform cameraTransform = Camera.main == null ? null : Camera.main.transform;

            foreach (TextMesh textMesh in GetComponentsInChildren<TextMesh>())
            {
                if (cameraTransform != null)
                    textMesh.transform.rotation = cameraTransform.rotation;
            }
        }

        private void UpdateVisuals()
        {
            if (healthFill != null)
            {
                float ratio = maxHealth <= 0f ? 0f : health / maxHealth;
                healthFill.localScale = new Vector3(0.94f * ratio, 0.64f, 0.55f);
                healthFill.localPosition = new Vector3(-0.47f * (1f - ratio), 0f, -0.06f);
            }

            if (shieldRing != null)
                shieldRing.gameObject.SetActive(shield > 0f);
        }

        private static Material CreateMaterial(Color color)
        {
            Shader shader = Shader.Find("Universal Render Pipeline/Lit");

            if (shader == null)
                shader = Shader.Find("Standard");

            Material material = new Material(shader);
            material.color = color;
            return material;
        }

        private static Vector3 CellStandingPosition(AutoChessBoardCell cell)
        {
            return cell.transform.position + Vector3.up * 0.78f;
        }
    }
}
