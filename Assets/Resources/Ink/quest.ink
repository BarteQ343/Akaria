INCLUDE globals.ink

-> talk_to_andrew

=== talk_to_andrew ===
{quest_ended:
-> quest_end
- else:
-> questing 
}

=== questing ===
{quest_completed: 
-> quest_complete
- else: 
-> questing_2
}

=== questing_2 === 
{quest_started:
-> placeholder_dialogue
- else:
-> start
}

=== start ===
# speaker: Andrew
Hello there, adventurer! Looking for work?
* [Yes] -> yes_work
* [No] -> no_work

=== no_work ===
# speaker: Tom
Thanks, I'm good.
# speaker: Andrew
Well, that's too bad. If you change your mind, you know where to find me.
-> END

=== yes_work ===
~ quest_started = true
# speaker: Tom
Maybe. What do you need?
# speaker: Andrew
There's something strange happening in the forest. I need you to check it out.
# speaker: Tom
I'll get to it.
# speaker: Andrew
Thanks a lot! Let me know if you find anything.
-> END

=== placeholder_dialogue ===
# speaker: Andrew
I'm counting on you!
-> END

=== quest_complete ===
~ quest_ended = true
# speaker: Tom
I'm back.
# speaker: Andrew
And? What did you find?
# speaker: Tom
Just some skeletons walking around. Took care of them.
# speaker: Tom
Other than that? Not much.
# speaker: Andrew
I see... Well, thank you for taking care of the monsters. I'll let you know if I'll need anything else.
# speaker: Tom
Sure thing. Goodbye.
-> END

=== quest_end ===
# speaker: Andrew
Thanks again for your help!
-> END
