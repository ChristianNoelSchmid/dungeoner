using Dungeoner.Importers;
using Dungeoner.Maps;
using Dungeoner.TokenManipulation;
using Godot;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Dungeoner.Ui;

public partial class UiTokenList : UiList
{
    private TokenImporter _importer = default!;
    private TokenMap _tokenMap = default!;
    private SelectionTool _selectionTool = default!;
    private UserMap _userMap = default!;
    private NetworkManager _netManager = default!;
    private bool _mouseOver = false;
    private bool _draggingOffList = false;
    List<TokenInstance> _tokenInstances = new();

    public TokenType PlacingTokenType { get; set; } = TokenType.World;

    public override void _Ready()
    {
        _importer = (TokenImporter)GetNode("/root/Main/Importers/TokenImporter");
        _tokenMap = (TokenMap)GetNode("/root/Main/TokenMap");
        _selectionTool = (SelectionTool)GetNode("/root/Main/SelectionTool");
        _userMap = (UserMap)GetNode("/root/Main/UserMap");
        _netManager = (NetworkManager)GetNode("/root/Main/NetworkManager");
    }

    public override void _Process(double delta)
    {
        if (Input.IsKeyPressed(Key.Escape)) { ReleaseFocus(); }
        if (Input.IsActionJustPressed("select") && !_mouseOver) ReleaseFocus();
        if (Input.IsActionJustPressed("select") && _mouseOver) _draggingOffList = true;
        if (Input.IsActionJustReleased("select")) _draggingOffList = false;
    }

    public override void QueryList(params string[] globs)
    {
        Clear();
        List<TokenInstance> instances = new();
        foreach (var glob in globs)
        {
            instances.AddRange(_importer.GetAllMatchingMetas(glob));
        }
        _tokenInstances.Clear();
        foreach (var instance in instances.Distinct())
        {
            _tokenInstances.Add(instance);
            AddIconItem(instance.Texture);
        }
    }

    public void OnMouseEnter() => _mouseOver = true;
    public void OnMouseExit()
    {
        _mouseOver = false;
        var mousePosition = _tokenMap.GetGlobalMousePosition();

        if (_draggingOffList)
        {
            var tokens = new List<Token>();
            foreach (int idx in GetSelectedItems())
            {
                var id = Guid.NewGuid();

                var token = _importer.GetToken(_tokenInstances[idx]);

                token.Teleport(mousePosition);
                tokens.Add(token);
                token.TokenType = PlacingTokenType;

                _tokenMap[id] = token;

                token.Visible = false;
            }
            _selectionTool.SelectTokens(tokens);
            _selectionTool.JustPlacing = true;
        }
        _draggingOffList = false;
    }
}
