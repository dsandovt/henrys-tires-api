// Migration: Add Address and Phone to branch documents
// Run with: mongosh "your-connection-string" migrations/001-add-branch-addresses.js

const db = db.getSiblingDB("Inventory");

const branchUpdates = [
  {
    code: "WARWICK",
    address: "15010 Warwick Blvd",
    phone: "(757) 234-6333"
  },
  {
    code: "WILLIAMSBURG",
    address: "7106 Merrimac Trail",
    phone: "(757) 585-2329"
  },
  {
    code: "JEFFERSON",
    address: "11239 Jefferson Avenue",
    phone: "(757) 592-9345"
  },
  {
    code: "MERCURY",
    address: "4002 West Mercury Blvd",
    phone: "(757) 826-5295"
  }
];

for (const update of branchUpdates) {
  const result = db.Branch.updateOne(
    { Code: update.code },
    { $set: { Address: update.address, Phone: update.phone } }
  );
  print(`Updated ${update.code}: matched=${result.matchedCount}, modified=${result.modifiedCount}`);
}

print("Branch address migration complete.");
