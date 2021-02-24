﻿using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;

namespace Moggle
{

public record FixedGameMode : IMoggleGameMode
{
    private FixedGameMode() { }
    public static FixedGameMode Instance { get; } = new();

    /// <inheritdoc />
    public string Name => "Fixed";

    /// <inheritdoc />
    public (MoggleBoard board, Solver Solver, TimeSituation TimeSituation)
        CreateGame(
            ImmutableDictionary<string, string> settings,
            Lazy<WordList> wordList)
    {
        var letters = Letters.Get(settings)
            .EnumerateRunes()
            .Select(Letter.Create)
            .ToImmutableArray();

        var c = Coordinate.GetMaxCoordinateForSquareGrid(letters.Length);

        var total = (c.Column + 1) * (c.Row + 1);

        if (letters.Length < total)
        {
            letters = letters.AddRange(Enumerable.Repeat(PaddingLetter, total - letters.Length));
        }

        var board = new MoggleBoard(letters, c.Column + 1);

        int? minWordLength = MinWordLength.Get(settings);

        if (minWordLength < 0)
            minWordLength = null;

        var minimum = Minimum.Get(settings);
        var maximum = Minimum.Get(settings);

        (int, int)? range;

        if (minimum < 0)
            range = null;
        else
            range = (minimum, maximum);

        var allowEquations = letters.Any(x => x.WordText.Equals("="));

        var solveSettings = new SolveSettings(minWordLength, allowEquations, range);

        var    ts = TimeSituation.Create(TimeSituation.Duration.Get(settings));
        Solver solver;

        var wordsToAnimate = WordsToAnimate.Get(settings);

        if (string.IsNullOrWhiteSpace(wordsToAnimate))
        {
            solver    = new Solver(wordList, solveSettings);
        }
        else
        {
            var words = wordsToAnimate.Split(
                ' ',
                StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries
            );
            solver    = new Solver(wordList.Value.AddWords(words), solveSettings);
        }

        return (board, solver, ts);
    }

    /// <inheritdoc />
    public Animation? GetAnimation(
        ImmutableDictionary<string, string> settings,
        Lazy<WordList> wordList)
    {
        var letters = Letters.Get(settings)
            .EnumerateRunes()
            .Select(Letter.Create)
            .ToImmutableArray();

        var c = Coordinate.GetMaxCoordinateForSquareGrid(letters.Length);

        var total = (c.Column + 1) * (c.Row + 1);

        if (letters.Length < total)
        {
            letters = letters.AddRange(Enumerable.Repeat(PaddingLetter, total - letters.Length));
        }

        var board = new MoggleBoard(letters, c.Column + 1);

        var wordsToAnimate = WordsToAnimate.Get(settings);

        if (string.IsNullOrWhiteSpace(wordsToAnimate))
            return null;

        var words = wordsToAnimate.Split(
            ' ',
            StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries
        );

        return Animation.Create(words, board);
    }

    private static readonly Letter PaddingLetter = Letter.Create("😊".EnumerateRunes().Single());

    public static readonly Setting.String Letters =
        new(nameof(Letters), @".+", "tarantula", "Exact Grid Letters")
        {
            GetRandomValue = GoodSeedHelper.GetGoodSeed
        };

    public static readonly Setting.Integer MinWordLength = new(nameof(MinWordLength), -1, 8, 3);

    public static readonly Setting.Integer Minimum = new(nameof(Minimum), -1, int.MaxValue, -1);
    public static readonly Setting.Integer Maximum = new(nameof(Maximum), -1, int.MaxValue, -1);

    public static readonly Setting.String WordsToAnimate = new(
        nameof(WordsToAnimate),
        null,
        "",
        "Words to Animate"
    );

    /// <inheritdoc />
    public IEnumerable<Setting> Settings
    {
        get
        {
            yield return Letters;
            yield return MinWordLength;
            yield return Minimum;
            yield return Maximum;
            yield return TimeSituation.Duration;
            yield return WordsToAnimate;
        }
    }
}

}
