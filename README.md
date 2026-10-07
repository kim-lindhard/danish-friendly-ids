# Danish Friendly IDs

A .NET library for human-friendly Danish identifiers: a person can be *glade danser* or
*energiske maler*, an object or a case *blå kasse*.

**Status: Stage 2.** The generator works, and every Danish word is categorised and available in code.
Packaging (Stage 3) comes next.

## Generating identifiers

```csharp
var ids = new FriendlyIdGenerator();                       // embedded words, Random.Shared

FriendlyId person = ids.Next(IdKind.Person);                        // "glade danser" or "dansende pilot"
FriendlyId thing = ids.Next(IdKind.Object);                         // "elektriske lampe"
ids.Next(IdKind.Person, IdFormat.ThreeWords);                       // "glade dansende pilot"
ids.Next(IdKind.Object, IdFormat.TwoWords.WithNumber(99));          // "blå kasse 42"

if (ids.TryNext(IdKind.Person, IdFormat.ThreeWords, id => store.Exists(id.ToString()), out var free))
    store.Add(free.ToString());                                     // up to 1,000 tries

long space = ids.CapacityOf(IdKind.Person, IdFormat.ThreeWords);    // exact
ids.Next(IdKind.Person.WithLessCommonWords());                      // also draws approved less common words

var id = ids.Next(IdKind.Person, IdFormat.ThreeWords.WithNumber(99));
id.ToString();                                  // "kløgtige dansende pilot 42"
id.ToString(IdStyle.Ascii);                     // "kloegtige dansende pilot 42"
id.ToString(IdStyle.UrlSlug);                   // "kloegtige-dansende-pilot-42"
ids.TryResolve(IdKind.Person, "kloegtige-dansende-pilot-42", out var back);   // back to "kløgtige dansende pilot 42"

var vehicle = new IdKind("Køretøj", Vocabulary.Objects, [MeaningCategory.Colour], [MeaningCategory.Vehicle]);
ids.Next(vehicle);                                         // "røde traktor"
```

- **Identifiers are random.** Uniqueness is the caller's job: store what you hand out, and use `TryNext` to retry against it.
- **Adjectives are always in the definite form** (*glade*, *blå*, *runde*), which is the same for both genders.
  Verbs appear as **-ende words** (*dansende*, *blinkende*), which never inflect.
- **Formats:**
  - `IdFormat.TwoWords` (the default) is an adjective or -ende word + a noun;
  - `IdFormat.ThreeWords` is an adjective + an -ende word + a noun;
  - `.WithNumber(n)` appends a number from 1 to n;
  - no identifier repeats a word.
- **Styles:**
  - `IdStyle.Danish` (the default) writes æ, ø, å with spaces;
  - `IdStyle.Ascii` folds them to ae, oe, aa, for systems that don't allow æøå;
  - `IdStyle.UrlSlug` is ASCII with hyphens: only a–z, 0–9 and `-`, all unreserved in a URL, so a slug never needs percent-encoding.

  No two approved words in a list fold to the same spelling (a test guards this), so every style is as unique as the Danish one, and the counts are the same. `TryResolve(kind, text)` reads any style back to the Danish identifier, or returns false if the kind could not have made it.
- **How many?** `CapacityOf(kind, format)` counts exactly: blocked pairs and repeated words are subtracted.
  `dotnet run --project DanishFriendlyIds.Sample -- capacity` prints the table below.

**No identifier is ever negative.** Two layers make sure of that:

1. **Rules.** A word must pass all of these:
   - it is 3–12 lowercase Danish letters;
   - it has no restriction (marked usage, rare, trademark);
   - no sense has a sexual or ethnicity topic;
   - centrality ≥ 1 (common words). `kind.WithLessCommonWords()` lowers this to 0, so approved peripheral words are drawn too. Same review, same never-negative rules; just more words;
   - no sentiment below 0. For `Person` this holds for every sense of the word. For `Object` it holds for the sense the word is used in, so an adjective that is negative only about people can still describe a thing.

   `Person` also excludes any word with a sense about geography (place of origin), religion, politics or medicine.
   `Object` excludes nouns that can also name a person (*bager* is the baker and the bakery), so a colour can never read as someone's skin.
2. **Review.** Each kind draws from its own reviewed list: `Person` from `DanishFriendlyIds/Data/review-people.tsv`, `Object` from `review-objects.tsv`. Only words marked `approved` in that list are drawn, and a word with no verdict there is never drawn. A word can be approved in one list and rejected in the other: *energiske* fits a person, *elektriske* a thing. The review also rejects:
   - weapons and violence;
   - bodies, appearance and skin colour;
   - death, illness and misfortune;
   - odd or unclear words.

   A `pair` line blocks two words that are fine alone but insulting together, wherever they appear in an
   identifier in that order (adjective + noun, -ende word + noun, adjective + -ende word).

| Kind | Adjectives (a sense that is …) | -ende words (a verb sense that is …) | Nouns (a sense that is …) |
|---|---|---|---|
| `IdKind.Person` (people list) | Mental or Physical, not Condition | Act, Communication, Experience, Mental or Social, not Condition | Human, not Group/Institution |
| `IdKind.Object` (objects list) | Physical or Colour, not Condition | Physical, Event or Existence, not Condition | Container, Furniture, Instrument, Vehicle, Garment, Building or Comestible; no sense of the word names a person |

**How many** (from `Sample -- capacity`, 2026-10-07):

| | 2 words | 2 words + 1–9 | 2 words + 1–99 | 3 words | 3 words + 1–9 | 3 words + 1–99 |
|---|---:|---:|---:|---:|---:|---:|
| `Person` (61 adjectives, 66 -ende words, 275 nouns) | 34,615 | 311,535 | 3,426,885 | 1,103,819 | 9,934,371 | 109,278,081 |
| `Object` (67 adjectives, 23 -ende words, 599 nouns) | 53,880 | 484,920 | 5,334,120 | 917,631 | 8,258,679 | 90,845,469 |
| `Person.WithLessCommonWords()` (81 adjectives, 93 -ende words, 723 nouns) | 124,956 | 1,124,604 | 12,370,644 | 5,432,644 | 48,893,796 | 537,831,756 |
| `Object.WithLessCommonWords()` (195 adjectives, 40 -ende words, 2,458 nouns) | 576,097 | 5,184,873 | 57,033,603 | 19,050,519 | 171,454,671 | 1,886,001,381 |

### The review file

Both review files have the columns `word`, `word_class` (`adj`, `part` for -ende words, `sb` or `pair`), `verdict` (`approved` or
`rejected`) and `reason`, which is required for a rejection. `word` is the form shown in the identifier:
the definite adjective (*glade*), the noun (*danser*), or for a pair both (*tomme tønde*).

The first verdicts came from two independent reviews: a word is approved only if both kept it. Of 2,442
candidates, 975 were approved and 1,467 rejected, each with its reason, and four pairs are blocked. The
verdicts were then split into the two lists; verdicts marked `kim` are the owner's own and final.

Verdicts are about the word, not the kind. A project that defines its own kind says which list it draws from (`Vocabulary.People` or
`Vocabulary.Objects`); its words must be approved there.

When COR is updated, or a project defines its own `IdKind`, new candidates have no verdict and are not
used until someone reviews them:

```
dotnet run --project DanishFriendlyIds.Sample -- unreviewed     # TSV of candidates without a verdict
```

To see what the generator makes: `dotnet run --project DanishFriendlyIds.Sample -- [count=50] [seed]`.

## The word list

`data/danish-words.tsv` has one row per headword and word class. It holds 94,047 rows for the 93,811
headwords of the official spelling dictionary (COR, 64,253) and Den Danske Ordbog's extra headwords
(COR.EXT, 29,558). About 34,000 common headwords also carry their meaning, taken from COR.SEM.

| Column | Meaning |
|---|---|
| `id` | `COR.<n>` or `COR.EXT.<n>`. Shared by the two rows of a headword under two classes (*ifølge*: preposition + abbreviation) |
| `lemma` | The headword |
| `word_class` | COR label: `sb` noun, `adj`, `vb`, `adv`, `prop`, `fork` abbreviation … (21 in all) |
| `definite_form` | Adjectives only: *glade*, *kolde*, *blå*. Blank for 109, mostly comparatives and quantity words |
| `categories` | Atoms of the COR.SEM ontological types across all senses: `Human`, `Occupation`, `Container`, `Mental`, `Colour` … (62 in all) |
| `topics` | Subject-field codes from Den Danske Ordbog: `zoo`, `med`, `mad`, `spo` … |
| `min_sentiment` | The lowest sentiment of any sense, from −3 to 3 |
| `centrality` | The highest centrality of any sense, from 0 to 3; higher means more core vocabulary |
| `restriction` | `sprogbrug` (marked usage) and `frekvens` (rare) when every sense carries them; `varemærke` for registered trademarks |
| `senses` | Number of COR.SEM senses; 0 means no meaning data |
| `present_participle` | Verbs only: the -ende form (*danse* → *dansende*). 7,475 verbs have one |

The meaning columns above summarise across senses. `data/danish-senses.tsv` holds each sense on its own
line, 42,711 in all. Its columns are `id`, `word_class`, `sense` (1, 2, …), `categories`, `topics`,
`sentiment`, `centrality` and `restriction`. The library reads meaning from this file, so a filter can ask
what one sense is: *rød* is a colour in one sense and "red in the face" in another.

In code:

```csharp
var lexicon = Lexicon.Embedded;
var moodAdjectives = lexicon.Of(WordClass.Adjective)
    .Where(word => word.Is(MeaningCategory.Mental) && word.Centrality >= 1);
var happy = lexicon.Find("glad").Single(word => word.WordClass == WordClass.Adjective);   // DefiniteForm "glade"
```

## Rebuilding

```
dotnet run --project DanishFriendlyIds.WordListBuilder
```

The builder downloads the sources into `data/sources/` and checks each one against its pinned SHA-256.
It then writes `data/danish-words.tsv` and `DanishFriendlyIds/Data/danish-words.tsv.gz`, and prints
counts per word class. When ordregister.dk publishes a new version, update `SourceFile.cs` and rerun.

## Data and credits

All data comes from [Det Centrale Ordregister](https://ordregister.dk) and is released under
[CC0](https://creativecommons.org/publicdomain/zero/1.0/):
- COR 1.5.1.0 (Retskrivningsordbogen 5.1), Dansk Sprognævn
- COR.EXT 1.0, Det Danske Sprog- og Litteraturselskab (DSL)
- COR.SEM 1.0, DSL and Center for Sprogteknologi, University of Copenhagen (CST)

CC0 asks for no attribution; the credit is given anyway.
