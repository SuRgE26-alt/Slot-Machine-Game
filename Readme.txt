# Slot Machine

## Game Overview

A 3-reel slot machine built in Unity. The player starts with a set amount of coins and picks a bet (Low or High) before pulling the handle. Each row spins independently and slows down to land on a random symbol. Once all three rows stop, the game checks for matches and pays out coins based on the symbols landed and the bet chosen.

- Click the handle to spin (raycast-based, so it only triggers if you actually click the handle sprite).
- Two bet tiers — Low and High — where High costs more but multiplies the payout.
- Coins display animates when spent or earned instead of snapping instantly.
- Landing all three rows on the same symbol (three of a kind) wins the game and loads the Win scene.
- Running out of coins loads the Game Over scene.

## Instructions to Run WebGL Build

1. Open the project in Unity (tested in [your Unity version here]).
2. Go to `File → Build Settings`.
3. Select **WebGL** as the platform (switch platform if it isn't already selected).
4. Make sure all scenes (Main game scene, WinScene, GameOverScene) are added under "Scenes in Build."
5. Click **Build**, choose an output folder.
6. Once the build finishes, open the generated `index.html` in a browser, or serve the build folder with a local server (WebGL builds won't run correctly opened directly as a file in some browsers due to CORS — use something like `python -m http.server` from inside the build folder if you hit issues).

## Bonus Features

- **Two bet tiers** with different payout multipliers, rather than a flat single bet.
- **Animated coin counter** — coins tick up/down one at a time instead of jumping straight to the new value.
- **Audio** — lever pull sound, a looping spin sound per reel, and a shared sound for both spending and earning coins.
- **Data-driven payouts** — the payout table is a list set up in the Inspector, so adding or changing symbol payouts doesn't need code changes.
- **Editor gizmos** on each row to visualize the start/end points and symbol spacing without entering Play Mode, plus live repositioning in the editor as spacing values are tweaked.
- **Win/Game Over flow** — three of a kind ends the game with a win, running out of coins ends it with a loss.

## Thought Process / Approach

Started from a working but fairly rigid version (hardcoded float positions per symbol, a long if/else chain for payouts, polling for row state every frame). The main goals while reworking it were:

- **Make the symbol a row lands on come from the same source as its position**, instead of guessing the symbol from a float comparison on `transform.position.y`. Switched to an enum + index-based approach, so the displayed symbol and the logical symbol can never disagree.
- **Make payouts configurable instead of hardcoded.** Replaced the long if/else chain with a list of payout rules (symbol, match count, payout) that can be edited directly in the Inspector.
- **Event-driven over polling.** Rows now fire an event when they stop, and `GameController` only checks the result once all three have reported in, instead of checking booleans every frame in `Update()`.
- **Iterated based on visual issues as they came up** — when reel spacing started drifting visually after switching from a single repositioned sprite to separate child sprites per symbol, added gizmos and an editor-time repositioning pass so spacing could be tuned by eye without needing to enter Play Mode each time.
- **Kept the betting and scene-transition logic intentionally simple** — two fixed bet tiers rather than a generic system, and direct `SceneManager.LoadScene` calls for win/lose rather than a more abstract state machine, since the scope of the project didn't call for more than that.
