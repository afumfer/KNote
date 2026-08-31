const readline = require("node:readline/promises");
const { stdin: input, stdout: output } = require("node:process");

(async () => {
    const rl = readline.createInterface({ input, output });
    const name = await rl.question("Enter your name: ");

    console.log(`Hello, ${name}!`);

    for (let i = 1; i <= 3; i++) {
        console.log(`Line ${i}`);
    }

    console.log("<< end >>");
    await rl.question("Press Enter to close...");
    rl.close();
})();
