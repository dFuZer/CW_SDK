using UnityEngine;
using System.Reflection;
using System.Linq;
using DbsContentApi;

namespace ExampleMod;

public static class CustomContent
{
    private const string BundleNameMod = "example_mod";
    private const string BundleNameMap = "example_scene";
    public static ShopItemCategory TestCategory;
    public static AssetBundle? BundleMod { get; private set; }
    public static AssetBundle? BundleMap { get; private set; }


    public static void Init()
    {
        Logger.Log("Loading custom content...");

        BundleMod = ContentLoader.LoadAssetBundle(Assembly.GetExecutingAssembly(), BundleNameMod);
        if (BundleMod == null)
        {
            Logger.LogError($"Failed to load asset bundle '{BundleNameMod}'.");
            return;
        }

        BundleMap = ContentLoader.LoadAssetBundle(Assembly.GetExecutingAssembly(), BundleNameMap);
        if (BundleMap == null)
        {
            Logger.LogError($"Failed to load asset bundle '{BundleNameMap}'.");
            return;
        }

        RegisterMap();
        RegisterMob();
        RegisterItems();

        Logger.Log("Custom content successfully initialized.");
    }

    private static void RegisterMap()
    {
        string? scenePath = Maps.FindScenePath(BundleMap!, "TestScene");
        if (scenePath != null)
        {
            Maps.RegisterMap(BundleMap!, scenePath, "Example Map");
        }
        else
        {
            Logger.LogWarning("ExampleMap scene not found in bundle.");
        }
    }

    private static void RegisterMob()
    {
        var config = new MobSetupConfig
        {
            visualRig = null,
            budget = new BudgetConfig { budgetCost = 1, rarity = 1f },
            controller = new ControllerConfig(),
            player = new PlayerConfig(),
            ragdoll = new RagdollConfig(),
            photonView = new PhotonViewConfig(),
            bot = new BotConfig(),
            navMesh = new NavMeshAgentConfig { height = 2f, radius = 1f, speed = 3.5f },
            addMonsterSyncer = true,
            addAnimRefHandler = true,
            addMonsterAnimationHandler = true,
            addHeadFollower = true,
            addGroundPos = true,
        };

        GameObject? mobPrefab = ContentLoader.LoadPrefabFromBundle(BundleMod!, "DefaultCharacter.prefab");
        if (mobPrefab == null)
        {
            Logger.LogWarning("DefaultCharacter.prefab not found in bundle.");
            return;
        }

        Mobs.RegisterMonster(mobPrefab, "DefaultCharacter", config,
            material: GameMaterial.M_Monster,
            postSetup: go =>
            {
                GameObject botTestChar = Mobs.GetBotChildObject(go);
                Mobs.AddBotChaserComponent(botTestChar, new BotChaserConfig { targetDistance = 1.5f });

                var attackMeleeComponent = botTestChar.AddComponent<Attack_Melee>();
                attackMeleeComponent.damage = 50f;
                attackMeleeComponent.knockback = 5f;
                attackMeleeComponent.fallTime = 1f;
                attackMeleeComponent.cooldown = 1.5f;
                attackMeleeComponent.range = 2.5f;
                attackMeleeComponent.force = 800f;
                attackMeleeComponent.attackPart = BodypartType.Elbow_L;
                attackMeleeComponent.attackCurve = AnimationCurve.Constant(0f, 1f, 0.5f);
                attackMeleeComponent.collisionThreshold = 2f;
                attackMeleeComponent.additionalAttackPart = [];
                attackMeleeComponent.additionalAttackForceMultiplier = 0.5f;
            });
    }

    private static void RegisterItems()
    {
        TestCategory = Items.RegisterCustomCategory("Test Category");

        Items.DeferRegistration(() =>
        {
            RegisterTestItem();
            RegisterTestGun();
        });
    }

    private static GameObject? GetDogLaserProjectileSource()
    {
        GameObject? dogPrefab = Resources.Load<GameObject>("Dog");
        if (dogPrefab == null)
        {
            Logger.LogError("Dog prefab not found in Resources.");
            return null;
        }

        Transform? botTransform = dogPrefab.transform.Find("Bot")
            ?? dogPrefab.transform.Find("Bot_Dog");
        if (botTransform == null)
        {
            Logger.LogError("Bot child not found on Dog prefab.");
            return null;
        }

        Attack_Dog? attackDog = botTransform.GetComponent<Attack_Dog>();
        if (attackDog == null)
        {
            Logger.LogError("Attack_Dog component not found on Dog Bot.");
            return null;
        }

        if (attackDog.projectile == null)
        {
            Logger.LogError("Attack_Dog.projectile is null on Dog prefab.");
            return null;
        }

        return attackDog.projectile;
    }

    private static GameObject? CreateTestGunProjectilePrefab(GameObject source)
    {
        GameObject shotPrefab = Object.Instantiate(source);
        shotPrefab.name = "TestGunLaser";

        foreach (Projectile existing in shotPrefab.GetComponents<Projectile>())
        {
            Object.DestroyImmediate(existing);
        }

        Projectile proj = shotPrefab.AddComponent<Projectile>();
        proj.velocity = 200f;
        proj.gravity = 0f;
        proj.upVelocity = 0f;
        proj.fall = 0f;
        proj.force = 0f;
        proj.damage = 0f;
        proj.postHitBehavior = Projectile.PostHitBehaviour.Destroy;
        proj.layerType = HelperFunctions.LayerType.Tangible;

        var projectileLogic = shotPrefab.AddComponent<TestGunProjectileLogic>();
        projectileLogic.force = 40f;
        projectileLogic.fall = 2f;

        shotPrefab.SetActive(false);
        Object.DontDestroyOnLoad(shotPrefab);
        Logger.Log($"Created Test Gun projectile from Dog laser: {source.name}");
        return shotPrefab;
    }

    private static void RegisterTestItem()
    {
        GameObject? itemPrefab = ContentLoader.LoadPrefabFromBundle(BundleMod!, "TestItem.prefab");
        if (itemPrefab == null)
        {
            Logger.LogWarning("ExampleItem.prefab not found in bundle.");
            return;
        }

        itemPrefab.AddComponent<BasicUsableItem>();
        GameMaterials.ApplyMaterial(itemPrefab, GameMaterial.M_Apple_1, true);

        Sprite? icon = BundleMod!.LoadAsset<Sprite>("example_icon");
        Items.RegisterItem(itemPrefab, new ItemConfig
        {
            displayName = "Test Item",
            price = 100,
            category = TestCategory,
            icon = icon,
            holdPos = new Vector3(0.3f, -0.3f, 0.7f)
        });
    }

    private static void RegisterTestGun()
    {
        GameObject? laserSource = GetDogLaserProjectileSource();
        if (laserSource == null)
        {
            return;
        }

        GameObject? shotPrefab = CreateTestGunProjectilePrefab(laserSource);
        if (shotPrefab == null)
        {
            return;
        }

        GameObject? itemPrefab = ContentLoader.LoadPrefabFromBundle(BundleMod!, "TestGun.prefab");
        if (itemPrefab == null)
        {
            Logger.LogWarning("TestGun.prefab not found in bundle.");
            return;
        }

        var behaviour = itemPrefab.AddComponent<TestGunItemBehaviour>();
        behaviour.projectilePrefab = shotPrefab;

        Transform? firePoint = itemPrefab.transform.Find("FirePoint");
        if (firePoint == null)
        {
            Logger.LogWarning("FirePoint not found on TestGun; using item root transform.");
            behaviour.firePoint = itemPrefab.transform;
        }
        else
        {
            behaviour.firePoint = firePoint;
        }

        GameMaterials.ApplyMaterial(itemPrefab, GameMaterial.M_Metal, true);

        Sprite? icon = BundleMod!.LoadAsset<Sprite>("example_icon");
        Items.RegisterItem(itemPrefab, new ItemConfig
        {
            displayName = "Test Gun",
            price = 150,
            category = TestCategory,
            icon = icon,
            holdPos = new Vector3(0.3f, -0.3f, 0.7f),
            holdRot = new Vector3(0, 0, 0),
            useAlternativeHoldPos = true,
            alternativeHoldPos = new Vector3(0.2f, -0.22f, 0.7f)
        });
    }
}
