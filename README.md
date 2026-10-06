# Danish Friendly IDs

A .NET library for human-friendly Danish identifiers: a person can be *glade danser* or
*trætte cyklist*, a case *kolde kasse*.

**Status: Stage 2.** The generator works, and every Danish word is categorised and available in code.
Packaging (Stage 3) comes next.

## Generating identifiers

```csharp
var ids = new FriendlyIdGenerator();                       // embedded words, Random.Shared

FriendlyId person = ids.Next(IdKind.Person);               // "glade danser"
FriendlyId @case = ids.Next(IdKind.Case);                  // "kolde kasse"

if (ids.TryNext(IdKind.Person, id => store.Exists(id.ToString()), out var free))   // up to 1,000 tries
    store.Add(free.ToString());

long space = ids.CapacityOf(IdKind.Person);                // adjectives × nouns

var vehicle = new IdKind("Køretøj", [MeaningCategory.Colour], [MeaningCategory.Vehicle]);
ids.Next(vehicle);                                         // "røde traktor"
```

- **Identifiers are random.** Uniqueness is the caller's job: store what you hand out, and use `TryNext` to retry against it.
- **Adjectives are always in the definite form** (*glade*, *kolde*, *røde*), which is the same for both genders.

| Kind | Adjectives (a sense that is …) | Nouns (a sense that is …) | Size |
|---|---|---|---|
| `IdKind.Person` | Mental | Human, not Group/Institution | 235 × 739 ≈ 174k |
| `IdKind.Case` | Physical or Colour, not Condition | Container, Furniture, Instrument, Vehicle, Garment, Building or Comestible; not Human | 429 × 1,285 ≈ 551k |

**Rules that always apply.** A word qualifies when *one* of its senses fits the kind. The safety rules
apply to the *whole* word:
- it must be 3–12 lowercase Danish letters;
- no restriction (marked usage, rare, trademark);
- no sense with a sexual topic;
- centrality ≥ 1 by default;
- not in `DanishFriendlyIds/Data/blocklist.txt`.

`Person` also excludes words with any sense whose topic is geography (place of origin), religion,
politics, medicine or ethnicity, and words with any sense at sentiment −2 or below.

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
