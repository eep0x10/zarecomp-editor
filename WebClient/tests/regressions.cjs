const assert = require("node:assert/strict");
const fs = require("node:fs");
const path = require("node:path");
const ts = require("typescript");
const client = path.resolve(__dirname, "..");
const requireClient = require("node:module").createRequire(
  path.join(client, "package.json"),
);
let passed = 0;
function check(value, message) {
  assert.ok(value, message);
  passed++;
  console.log("PASS " + message);
}
function sourceModule(name) {
  const text = fs.readFileSync(path.join(client, "src", name), "utf8");
  const js = ts.transpileModule(text, {
    compilerOptions: {
      module: ts.ModuleKind.CommonJS,
      target: ts.ScriptTarget.ES2022,
      jsx: ts.JsxEmit.ReactJSX,
    },
  }).outputText;
  const m = { exports: {} };
  new Function("require", "module", "exports", js)(
    (id) =>
      id.startsWith("./")
        ? sourceModule(id.slice(2) + ".ts")
        : requireClient(id),
    m,
    m.exports,
  );
  return m.exports;
}
const text = fs.readFileSync(path.join(client, "src/main.tsx"), "utf8");
const tree = ts.createSourceFile(
  "main.tsx",
  text,
  ts.ScriptTarget.Latest,
  true,
  ts.ScriptKind.TSX,
);
function bind(name, context) {
  let found;
  function visit(node) {
    if (
      (ts.isFunctionDeclaration(node) || ts.isVariableDeclaration(node)) &&
      node.name?.getText(tree) === name
    )
      found = ts.isVariableDeclaration(node) ? node.initializer : node;
    ts.forEachChild(node, visit);
  }
  visit(tree);
  assert.ok(found, name);
  const js = ts.transpileModule("module.exports=" + found.getText(tree), {
      compilerOptions: {
        module: ts.ModuleKind.CommonJS,
        target: ts.ScriptTarget.ES2022,
      },
    }).outputText,
    m = { exports: {} };
  new Function(...Object.keys(context), "module", "exports", js)(
    ...Object.values(context),
    m,
    m.exports,
  );
  return m.exports;
}
async function main() {
  const helpers = sourceModule("alphaDraft.ts");
  // Captured from the real 2.1.0 API on the synthetic Chikorita Alpha fixture.
  // Keep the full nullable patch shape; sparse mocks missed this regression.
  const serializedPatch = JSON.parse(
    fs.readFileSync(
      path.join(client, "tests/fixtures/alpha-plan-nullable-patch.json"),
      "utf8",
    ),
  );
  const nullablePlan = {
    available: true,
    hash: "synthetic",
    mode: "Custom",
    patch: serializedPatch,
    options: null,
    preview: { values: { species: 152, form: 0 } },
  };
  const sparse = helpers.planDraft(nullablePlan);
  check(
    Object.values(serializedPatch).filter((v) => v === null).length === 30,
    "fixture retains all30 nullable members from a real API response",
  );
  check(
    !Object.hasOwn(sparse, "species") &&
      !Object.hasOwn(sparse, "form") &&
      !Object.hasOwn(sparse, "nickname"),
    "nullable plan members are no-ops, not identity overrides",
  );

  const meaningful = helpers.planDraft({
    ...nullablePlan,
    patch: {
      species: null,
      form: undefined,
      favorite: false,
      heldItem: 0,
      nickname: "",
      plusMoves: [],
    },
  });
  check(
    meaningful.favorite === false &&
      meaningful.heldItem === 0 &&
      meaningful.nickname === "" &&
      Array.isArray(meaningful.plusMoves) &&
      !meaningful.plusMoves.length,
    "sparse plan retains false, zero, empty text and empty arrays",
  );
  const identity = {
    species: 152,
    form: 0,
    gender: 0,
    level: 25,
    nature: 15,
    ball: 4,
    nickname: "Original",
    isAlpha: true,
    alphaMode: "Compatible",
  };
  const contextKey = "party:0",
    originalDraft = { nickname: "Kept draft", ball: 2, level: 26 };
  let actualDrafts = { [contextKey]: { ...originalDraft } },
    writes = 0;
  const undoStore = { current: {} };
  const applyUpdate = bind("update", {
    revision: { current: 0 },
    validationRequest: { current: null },
    setValidating: () => {},
    setPreview: () => {},
    setAlphaPlan: () => {},
    setAlphaPlanning: () => {},
    setError: () => {},
    creating: false,
    details: { values: identity },
    loadedKey: { current: contextKey },
    activeKey: contextKey,
    same: (a, b) => JSON.stringify(a) === JSON.stringify(b),
    setDrafts: (fn) => (actualDrafts = fn(actualDrafts)),
  });
  const apply = bind("applyAlphaPlan", {
    alphaPlan: nullablePlan,
    session: { hash: "synthetic" },
    planDraft: helpers.planDraft,
    alphaUndo: undoStore,
    editContext: contextKey,
    patch: { ...originalDraft },
    creating: false,
    details: { values: identity },
    creationSeed: null,
    update: applyUpdate,
    setCreationSeed: () => {},
    setPreview: () => {},
    setMessage: () => {},
    setMotion: () => {},
    t: (x) => x,
    api: () => {
      writes++;
      throw Error("Apply must not write");
    },
  });
  apply();
  const effective = { ...identity, ...actualDrafts[contextKey] };
  check(
    effective.species === 152 &&
      effective.form === 0 &&
      effective.gender === 0 &&
      effective.nature === 15,
    "real serialized Alpha Apply keeps inspector identity and sprite species",
  );
  check(
    effective.nickname === "Kept draft" &&
      effective.ball === 2 &&
      effective.level === 26,
    "real serialized Alpha Apply preserves unrelated existing draft fields",
  );
  check(
    effective.isAlpha === true &&
      effective.alphaMode === "Custom" &&
      writes === 0,
    "Apply changes only the local draft and never calls write API",
  );
  check(
    !Object.hasOwn(undoStore.current[contextKey].applied, "species"),
    "Undo does not claim null identity members as Alpha-owned changes",
  );
  const reverted = helpers.undoAlphaChanges(
    { ...effective, nickname: "Later edit" },
    undoStore.current[contextKey],
  );
  const afterUndo = { ...effective, nickname: "Later edit", ...reverted };
  check(
    afterUndo.species === 152 &&
      afterUndo.form === 0 &&
      afterUndo.nickname === "Later edit" &&
      afterUndo.ball === 2 &&
      afterUndo.alphaMode === "Compatible",
    "Undo after full-nullable Apply retains identity and later unrelated edits",
  );
  const creationPlan = {
    ...nullablePlan,
    options: {
      species: 4,
      form: 0,
      level: 25,
      nature: 15,
      ball: 2,
      shiny: false,
      creationSeed: "18446744073709551610",
    },
  };
  const creationBefore = {
    species: 4,
    form: 0,
    level: 10,
    nature: 15,
    ball: 2,
    heldItem: null,
    friendship: null,
    moves: null,
    nickname: "Creation draft",
  };
  const creationAfter = {
    ...creationBefore,
    ...helpers.planDraft(creationPlan),
  };
  check(
    creationAfter.species === 4 &&
      creationAfter.form === 0 &&
      creationAfter.level === 25 &&
      creationAfter.shiny === "NonShiny",
    "creation plan still fills required identity from options",
  );
  check(
    creationAfter.heldItem === null &&
      creationAfter.friendship === null &&
      creationAfter.moves === null &&
      creationAfter.nickname === "Creation draft",
    "incoming nulls preserve existing creation automatic fields and other overrides",
  );
  check(
    !helpers
      .alphaUnavailableMessage({
        mode: "Compatible",
        code: "alpha_encounter_unavailable",
      })
      .toLowerCase()
      .includes("level"),
    "missing species encounter never suggests raising level",
  );
  check(
    !helpers
      .alphaUnavailableMessage({
        mode: "Compatible",
        code: "alpha_origin_unavailable",
      })
      .toLowerCase()
      .includes("level"),
    "identity-incompatible origin never suggests raising level",
  );
  check(
    helpers
      .alphaUnavailableMessage({
        mode: "Compatible",
        code: "alpha_constraints_conflict",
      })
      .toLowerCase()
      .includes("level"),
    "field-constraint conflict can suggest level review",
  );
  check(
    !helpers
      .alphaUnavailableMessage({
        mode: "Custom",
        code: "alpha_constraints_conflict",
      })
      .includes("choose Custom"),
    "unavailable Custom does not recommend choosing Custom again",
  );
  const translatePlan = sourceModule("i18n.ts").translate;
  const planCatalog = {
    moves: [
      { id: 52, name: "Ember" },
      { id: 10, name: "Scratch" },
    ],
    natures: [{ id: 15, name: "Modest" }],
    balls: [{ id: 2, name: "Ultra Ball" }],
  };
  const label = (field, value, locale = "pt-BR") =>
    helpers.alphaChangeValue(field, value, planCatalog, (s) =>
      translatePlan(locale, s),
    );
  check(
    label("alphaMode", "null") === "—" &&
      label("alphaMode", "0") === "Encontro compatível" &&
      label("alphaMode", "1") === "Personalizado",
    "Alpha mode numeric/null values have readable localized names",
  );
  check(
    label("alphaMode", '"Custom"', "en") === "Custom" &&
      label("alphaMode", "Compatible", "es") === "Encuentro compatible",
    "Alpha mode string values remain supported",
  );
  check(
    label("isAlpha", "true") === "Sim" &&
      label("isAlpha", "false") === "Não" &&
      label("isAlpha", "true", "es") === "Sí",
    "plan booleans localized",
  );
  check(
    label("moves", "[52,10,0,9999]") === "Ember / Scratch / Nenhum / #9999",
    "move arrays use original English names, empty slots and honest unknown IDs",
  );
  check(
    label("metLocation", "Lumiose City (30026)") === "Lumiose City (30026)",
    "human location labels are not modified or given duplicate IDs",
  );
  check(
    label("nature", "15") === "Modest" && label("ball", "2") === "Ultra Ball",
    "nature and ball IDs render catalog names",
  );
  let value = helpers.natureChange(
    { nature: 0, statNature: 0 },
    15,
    false,
    false,
  );
  check(
    value.patch.statNature === 15,
    "Hardy without Mint follows changed Modest nature",
  );
  value = helpers.natureChange({ nature: 15, statNature: 10 }, 3, false, false);
  check(
    value.patch.statNature === 10,
    "independent Timid Mint survives original nature change",
  );
  value = helpers.natureChange({ nature: 15, statNature: 15 }, 10, true, false);
  check(
    value.patch.statNature === 15,
    "explicit same-value Modest Mint remains independent",
  );
  value = helpers.natureChange({ nature: 0, statNature: 0 }, 15, true, false);
  check(
    value.patch.statNature === 15 && value.resetMint,
    "unavailable neutral Mint repaired explicitly",
  );
  check(
    helpers.natureChange({ nature: 0 }, 15, false, true).patch.statNature ===
      null,
    "automatic creation Mint remains automatic",
  );
  const choices = sourceModule("PokemonChoices.tsx");
  const all = Array.from({ length: 25 }, (_, id) => ({ id, name: String(id) }));
  check(
    choices.mintChoices(all, 15).length === 21,
    "Mint offers20 amplified and Serious",
  );
  const translate = sourceModule("i18n.ts").translate;
  for (const [id, pt, es] of [
    [0, "Masculino", "Masculino"],
    [1, "Feminino", "Femenino"],
    [2, "Sem gênero", "Sin género"],
  ]) {
    const label = choices.genderChoiceLabel({
      id,
      name: "Bilingual legacy value",
    });
    check(
      translate("pt-BR", label) === pt &&
        translate("es", label) === es &&
        translate("en", label) === ["Male", "Female", "Genderless"][id],
      "gender " + id + " localized by ID in PT/EN/ES",
    );
  }
  const neutral = choices.mintChoices(all, 0).map((c) => c.id);
  check(
    neutral.includes(0) &&
      neutral.includes(12) &&
      !neutral.includes(6) &&
      !neutral.includes(18) &&
      !neutral.includes(24),
    "only matching original neutral remains selectable",
  );
  const plan = {
    patch: {
      isAlpha: true,
      alphaMode: "Compatible",
      level: 25,
      iVs: [31, 31, 31, 17, 19, 21],
    },
    options: {
      species: 4,
      form: 0,
      level: 25,
      nature: 15,
      ball: 2,
      shiny: false,
      creationSeed: "18446744073709551610",
    },
  };
  const draft = helpers.planDraft(plan);
  check(
    draft.species === 4 && draft.shiny === "NonShiny" && draft.level === 25,
    "server plan populates required creation fields",
  );
  check(
    !Object.hasOwn(draft, "creationSeed"),
    "creation seed never leaks into PokemonPatch",
  );
  const restored = helpers.undoAlphaChanges(
    {
      level: 25,
      metLocation: 9,
      nickname: "Later",
      isAlpha: false,
      alphaMode: null,
    },
    {
      before: { nickname: "Before" },
      applied: {
        level: 25,
        metLocation: 9,
        nickname: "Before",
        isAlpha: true,
        alphaMode: "Compatible",
      },
      baseline: { level: 10, metLocation: 1 },
    },
  );
  check(
    restored.level === 10 &&
      restored.metLocation === 1 &&
      !Object.hasOwn(restored, "nickname"),
    "Undo restores plan fields and retains later nickname edit",
  );
  check(
    !Object.hasOwn(restored, "isAlpha") &&
      !Object.hasOwn(restored, "alphaMode"),
    "Undo preserves a later explicit OFF edit",
  );
  const ordinaryUndo = helpers.undoAlphaChanges(
    { isAlpha: true, alphaMode: "Compatible" },
    {
      before: {},
      applied: { isAlpha: true, alphaMode: "Compatible" },
      baseline: { isAlpha: false, alphaMode: null },
    },
  );
  check(
    ordinaryUndo.isAlpha === false && ordinaryUndo.alphaMode === null,
    "non-Alpha plan Undo restores false/null",
  );
  const existingUndo = helpers.undoAlphaChanges(
    { isAlpha: true, alphaMode: "Custom" },
    {
      before: { isAlpha: true, alphaMode: "Compatible" },
      applied: { isAlpha: true, alphaMode: "Custom" },
      baseline: { isAlpha: true, alphaMode: null },
    },
  );
  check(
    existingUndo.isAlpha === true && existingUndo.alphaMode === "Compatible",
    "existing Alpha Custom-plan Undo restores true/Compatible",
  );
  let offPatch;
  await bind("alpha", {
    alphaPlan: null,
    alphaPlanning: false,
    update: (p) => (offPatch = p),
    setMessage: () => {},
    t: (x) => x,
  })(false);
  check(
    offPatch.isAlpha === false && offPatch.alphaMode === null,
    "explicit OFF always clears Alpha flag and mode",
  );
  const values = {
      species: 4,
      form: 0,
      level: 10,
      nature: 15,
      shiny: null,
      ball: 4,
    },
    val = (n, f = "") => values[n] ?? f,
    num = (n, f = 0) => Number(val(n, f));
  let options = bind("options", {
    num,
    val,
    destination: "Auto",
    preferredBox: -1,
    newPatch: values,
    creationSeed: "18446744073709551610",
  })();
  check(options.shiny === false, "automatic/null shiny is non-shiny");
  check(
    options.creationSeed === "18446744073709551610",
    "creation seed remains a lossless decimal string",
  );
  for (const style of ["Star", "Square"]) {
    values.shiny = style;
    options = bind("options", {
      num,
      val,
      destination: "Auto",
      preferredBox: -1,
      newPatch: values,
      creationSeed: null,
    })();
    check(options.shiny === true, style + " shiny preserved");
  }
  let live = { id: "same", hash: "NEW", box: ["fresh"] };
  const captured = { id: "same", hash: "OLD", summary: { boxCount: 3 } };
  const loadBox = bind("loadBox", {
    session: captured,
    boxRequest: { current: null },
    sessionId: { current: "same" },
    sessionHash: { current: "NEW" },
    api: async () => ({ entries: ["stale"], hash: "OLD" }),
    setSession: (fn) => (live = fn(live)),
    fail: (e) => {
      throw e;
    },
    ApiError: class extends Error {},
  });
  await loadBox(1);
  check(
    live.box[0] === "fresh" && live.hash === "NEW",
    "late box response cannot overwrite post-commit box",
  );
  let reject,
    errors = 0;
  const revision = { current: 1 };
  const validate = bind("validate", {
    session: { id: "s", hash: "h" },
    validationRequest: { current: null },
    revision,
    setValidating: () => {},
    setError: () => {},
    api: () => new Promise((a, b) => (reject = b)),
    creating: false,
    selection: { party: true, slot: 0 },
    patch: { level: 50 },
    setPreview: () => {},
    setMessage: () => {},
    t: (x) => x,
    fail: () => errors++,
  });
  const pending = validate();
  revision.current++;
  reject(new Error("old422"));
  await pending;
  check(
    errors === 0,
    "superseded validation failure cannot overwrite current feedback",
  );
  let changes = [],
    messages = [];
  const statValues = {
    isAlpha: true,
    iVs: [31, 31, 31, 17, 19, 21],
    hyperTrained: [false, false, false, false, false, false],
  };
  const statChange = bind("statChange", {
    val: (n, f) => statValues[n] ?? f,
    setError: (message) => messages.push(message),
    t: (x) => x,
    update: (p) => changes.push(p),
    set: (n, v) => changes.push({ [n]: v }),
  });
  statChange("iVs", 0, 0);
  check(
    !changes.length && messages.length === 1,
    "Alpha cannot reduce perfect IV count below3",
  );
  statChange("iVs", 3, 13);
  check(changes[0].iVs[3] === 13, "other Alpha IVs stay editable");
  check(
    /disabled=\{n === 0 && !!val\("isAlpha", false\)\}/.test(text),
    "IV0 preset is disabled for active Alpha",
  );
  let received;
  const alphaPlan = {
    available: true,
    hash: "hash",
    mode: "Compatible",
    patch: { isAlpha: true },
    options: null,
  };
  const planRevision = { current: 5 };
  const request = bind("requestAlphaPlan", {
    alphaChoice: "Compatible",
    session: { id: "s", hash: "hash" },
    operation: { current: false },
    clearValidationContext: () => planRevision.current++,
    setAlphaChoice: () => {},
    setAlphaPlanning: () => {},
    setError: () => {},
    validationRequest: { current: null },
    revision: planRevision,
    api: async () => alphaPlan,
    creating: false,
    selection: { party: true, slot: 0 },
    patch: { level: 22 },
    setAlphaPlan: (p) => (received = p),
    setMessage: () => {},
    t: (x) => x,
    fail: (e) => {
      throw e;
    },
    ApiError: class extends Error {},
  });
  await request();
  check(
    received === alphaPlan,
    "read-only plan reaches review without mutating the draft",
  );
  let drafts = {};
  const context = "party:0";
  const baseline = { nickname: "Original", isAlpha: true, alphaMode: "Custom" };
  const update = bind("update", {
    revision: { current: 0 },
    validationRequest: { current: null },
    setValidating: () => {},
    setPreview: () => {},
    setAlphaPlan: () => {},
    setAlphaPlanning: () => {},
    setError: () => {},
    creating: false,
    details: { values: baseline },
    loadedKey: { current: context },
    activeKey: context,
    same: (a, b) => JSON.stringify(a) === JSON.stringify(b),
    setDrafts: (fn) => (drafts = fn(drafts)),
  });
  update({ nickname: "New" });
  check(
    drafts[context].alphaMode === "Custom" &&
      !Object.hasOwn(drafts[context], "isAlpha"),
    "reopened Custom policy accompanies edits without a redundant Alpha flag",
  );
  update({ nickname: "Original" });
  check(
    !drafts[context],
    "reverting edits removes a redundant policy-only draft",
  );
  console.log(
    "SUCCESS " +
      passed +
      " client regression checks; HTTP/decode simulated, no save or preference IO.",
  );
}
main().catch((error) => {
  console.error(error);
  process.exitCode = 1;
});
