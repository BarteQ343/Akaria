INCLUDE globals.ink

{sylvie_met:
- 0: -> introduction
- 1: -> main_menu
}
=== introduction ===
# speaker: Sylvie
~ sylvie_met = 1
Ach, ty musisz być jednym z nowym członków gildii. Nazywam się Sylvie. Witamy na pokładzie!
-> main_menu

=== main_menu ===
# speaker: Sylvie
Czym mogę służyć?
* [Przyszedłem zatwierdzić zlecenie z tablicy.] -> accept_first_quest
+ [Czym się tutaj zajmujesz?] -> what_you_do
+ [Co możesz mi powiedzieć o tym miejscu?] -> tell_me_about_this_place
+ [Wyjdź] -> leave

=== leave ===
# speaker: Sylvie
Do widzenia! 
-> END

=== accept_first_quest ===
~ first_quest_status = 1
# speaker: Sylvie
Oczywiście. O którym zleceniu mowa?
# speaker: Finn
(Pokazujesz kartkę wziętą z tablicy) Chodzi o te związane z potworami w okolicy. Mam zniszczyć ich gniazdo, zgadza się?
# speaker: Sylvie
Tak jest. (Sylvie bierze kartkę do ręki i coś na niej zapisuje. Zatrzymuje się na moment, po czym zwraca się ponownie do ciebie)
Wybacz, ale jesteś tu nowy i zapomniałam twojego imienia. Potrzebuję go, aby oficjalnie przydzielić cię do tego zlecenia.
# speaker: Finn
Nic się nie stało. Jestem Finn.
# speaker: Sylvie
Finn... Mam to. Wszystko gotowe. Pomyślnych łowów!
# speaker: Finn
Dzięki.
-> DONE

=== what_you_do ===
# speaker: Sylvie
Jestem recepcjonistką gildii w tym miasteczku. Do moich obowiązków należy przyjmowanie zleceń dla gildii, przypisywanie ich do konkretnych członków gildii oraz wypłacanie nagrody za wykonane zlecenie. 
Oprócz tego zajmuję się również zbieraniem informacji przekazywanych nam przez poszukiwaczy i archiwizowaniu ich.
+ [Jakie informacje zbierasz od poszukiwaczy przygód?] -> what_info
+ [Zmieńmy temat.] -> main_menu
-> DONE

=== what_info ===
# speaker: Sylvie
Przede wszystkim informacje o napotkanych potworach. 
Ich zachowanie, styl walki i słabe punkty – ta wiedza uratowała wielu poszukiwaczy przygód! 
Gildia sporządziła oficjalny bestiariusz zawierający wszystkie szczegóły o okolicznych potworach na bazie tych informacji. 
{bestiary_bought: 
- 0: -> buy_question
- 1: -> main_menu
}

=== buy_question ===
Możesz go kupić u mnie.
* [Wezmę ten bestiariusz (100G)] -> buy
+ [Może innym razem] -> main_menu

=== buy ===
~ bestiary_bought = 1
# speaker: Sylvie
Niech ci służy!
-> main_menu

=== tell_me_about_this_place ===
# speaker: Sylvie
Znajdujemy się obecnie w miasteczku Ando, położonym na północny zachód od Jeziora Błękitnego Oka. 
Wielu poszukiwaczy przygód w regionie zaczynało właśnie tutaj, ponieważ większość zleceń jest dosyć łatwa. 
Oczywiście, znajdzie się również coś dla bardziej doświadczonych poszukiwaczy, za odpowiednio większą zapłatą.
# speaker: Finn
Na razie zostanę przy prostszych zleceniach. Nie ma co się rzucać na głęboką wodę.
# speaker: Sylvie
Ćwicz dalej, a z pewnością kiedyś będziesz w stanie podjąć się trudniejszych zleceń!
-> main_menu
