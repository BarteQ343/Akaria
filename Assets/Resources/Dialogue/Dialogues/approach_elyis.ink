INCLUDE globals.ink

{quest_prologue_approach_elyis:
    - 0: -> intro
    - 1: -> level_start
    - 2: -> approach_elyis
    - 3: -> village_search
}

=== intro ===
~ quest_prologue_approach_elyis = 1
Mieszkańcy wioski Elyis skarżą się na dziwne hałasy dochodzące z lasu. Niektórzy twierdzą, że widzieli jak coś się porusza w nocy. Starszy wioski prosi o zadanie sprawy. - treść zlecenia z tablicy ogłoszeń
-> END

=== level_start ===
~ quest_prologue_approach_elyis = 2
# speaker: Tavor
Elyis już niedaleko. Mam nadzieję, że nikt nie postanowił przeszukiwać lasu na własną rękę.
-> END

=== approach_elyis ===
~ quest_prologue_approach_elyis = 3
# speaker: Tavor
Cholera, spóźniłem się!
-> END

=== village_search ===

# speaker: Tavor
...
Muszę zdać raport gildii, okoliczne wioski mogą być w niebezpieczeństwie.
-> END