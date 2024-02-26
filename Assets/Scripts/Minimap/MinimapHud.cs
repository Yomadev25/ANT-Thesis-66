using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class MinimapHud : MonoBehaviour
{
    [Header("Map")]
    [SerializeField]
    private RectTransform _mapMarkerParentRectTransform;
    [SerializeField]
    private RawImage _mapImage;
    [SerializeField]
    private RectTransform _playerIconMapRectTransform;
    [SerializeField]
    private TMP_Text _mapNameText;

    [Header("Icons")]
    [SerializeField]
    private GameObject _interactMarkerIcon;
    [SerializeField]
    private GameObject _portalIcon;

    private Camera _mapCamera;
    private Transform _playerTransform;
    private List<(Interact interactivePosition, RectTransform markerRectTransform)> _currentMapInteractiveObjects = new();

    [Header("Zooming")]
    [SerializeField]
    private float _zoomSpeed = 0.1f;
    [SerializeField]
    private float _maxZoom = 10f;
    private Vector3 _initialScale;

    private void Awake()
    {
        _initialScale = _mapImage.rectTransform.localScale;

        MessagingCenter.Subscribe<Minimap, Camera>(this, Minimap.MessageInitMapCamera, (sender, camera) =>
        {
            InitMapCamera(camera);
            _mapNameText.text = sender.mapName;
        });

        /*MessagingCenter.Subscribe<Interact>(this, Interact.MessageOnDestroyInteractive, (sender) =>
        {
            RemoveInteractiveMarkerInMap(sender);
        });*/
    }

    private void OnDestroy()
    {
        MessagingCenter.Unsubscribe<Minimap, Camera>(this, Minimap.MessageInitMapCamera);
        //MessagingCenter.Unsubscribe<Interacable>(this, Interacable.MessageOnDestroyInteractive);
    }

    private void Start()
    {
        Transform playerTransform = GameObject.FindGameObjectWithTag("Player").transform;
        InitPlayerTransform(playerTransform);
    }

    private void Update()
    {
        if (_mapCamera == null) return;
        FollowPlayerTargetInMap();

        if (Input.mouseScrollDelta.y != 0)
        {
            OnScroll(Input.mouseScrollDelta.y);
        }
    }

    private void InitPlayerTransform(Transform playerTransform) => _playerTransform = playerTransform;
    private void InitMapCamera(Camera camera)
    {
        _mapCamera = camera;
        _mapImage.texture = camera.targetTexture;
        _mapMarkerParentRectTransform.localRotation = Quaternion.Euler(0, 0, 90f);
    }

    private void InitInteractiveMarkerDataInMap()
    {
        var interactObjs = FindObjectsOfType<Interact>();
        foreach (var interactObj in interactObjs)
        {
            //Assign icon follow interactive type
            GameObject icon = _interactMarkerIcon;

            //Spawn icon
            GameObject GO = Instantiate(icon, _mapMarkerParentRectTransform);
            RectTransform rectTransform = GO.GetComponent<RectTransform>();
            rectTransform.localScale = new Vector3(1.3f, 1.3f, 1.3f);
            _currentMapInteractiveObjects.Add((interactObj, rectTransform));

            //Active prefab
            GO.SetActive(true);

            //Init icon position
            if (_mapCamera == null)
            {
                Debug.LogWarning(nameof(_mapCamera) + "is not exist.");
                return;
            }

            foreach ((Interact interactivePosition, RectTransform markerRectTransform) marker in _currentMapInteractiveObjects)
            {
                Vector3 offset = marker.interactivePosition.transform.position - _mapCamera.transform.position;
                offset = offset / _mapCamera.orthographicSize * (_mapMarkerParentRectTransform.rect.height / 2);
                marker.markerRectTransform.anchoredPosition = new Vector2(offset.x, offset.z);
            }
        }
    }

    private void RemoveInteractiveMarkerInMap(Interact interactObj)
    {
        if (!_currentMapInteractiveObjects.Exists(interactive => interactive.interactivePosition == interactObj))
            return;

        (Interact pos, RectTransform rectTrans) foundObj = _currentMapInteractiveObjects.Find(interactive => interactive.interactivePosition == interactObj);
        Destroy(foundObj.rectTrans.gameObject);
        _currentMapInteractiveObjects.Remove(foundObj);
    }

    private void FollowPlayerTargetInMap()
    {
        if (_playerTransform == null)
        {
            Debug.LogWarning(nameof(_playerTransform) + "is not exist.");
            return;
        }

        Vector3 offset = _playerTransform.position - _mapCamera.transform.position;
        offset = offset / (_mapCamera.orthographicSize) * (_mapMarkerParentRectTransform.rect.height / 2);

        _playerIconMapRectTransform.anchoredPosition = new Vector2(offset.x, offset.z);
    }

    public void OnScroll(float scrollDelta)
    {
        var delta = Vector3.one * (scrollDelta * _zoomSpeed);
        var desiredScale = _mapImage.rectTransform.localScale + delta;
        desiredScale = ClampDesiredScale(desiredScale);
        _mapImage.rectTransform.localScale = desiredScale;
    }

    private Vector3 ClampDesiredScale(Vector3 desiredScale)
    {
        desiredScale = Vector3.Max(_initialScale, desiredScale);
        desiredScale = Vector3.Min(_initialScale * _maxZoom, desiredScale);
        return desiredScale;
    }
}