***REMOVED***
***REMOVED***
***REMOVED***

namespace Migration.Common
***REMOVED***
    public class Journal
    ***REMOVED***
***REMOVED***   #region Static methods
***REMOVED***   internal static Journal Init(MigrationContext context)
***REMOVED***   ***REMOVED***
***REMOVED******REMOVED***  var journal = new Journal(context);

***REMOVED******REMOVED***  if (File.Exists(journal.ItemsPath) && context.ForceFresh)
***REMOVED******REMOVED******REMOVED*** File.Delete(journal.ItemsPath);

***REMOVED******REMOVED***  if (File.Exists(journal.AttachmentsPath) && context.ForceFresh)
***REMOVED******REMOVED******REMOVED*** File.Delete(journal.AttachmentsPath);

***REMOVED******REMOVED***  return Load(journal);
***REMOVED***   ***REMOVED***

***REMOVED***   #endregion

***REMOVED***   #region parsing
***REMOVED***   internal static Journal Load(Journal journal)
***REMOVED***   ***REMOVED***
***REMOVED******REMOVED***  if (File.Exists(journal.ItemsPath))
***REMOVED******REMOVED***  ***REMOVED***
***REMOVED******REMOVED******REMOVED*** var revLines = File.ReadAllLines(journal.ItemsPath);
***REMOVED******REMOVED******REMOVED*** foreach (var rev in revLines)
***REMOVED******REMOVED******REMOVED*** ***REMOVED***
***REMOVED******REMOVED******REMOVED******REMOVED***var props = rev.Split(';');
***REMOVED******REMOVED******REMOVED******REMOVED***journal.ProcessedRevisions[props[0]] = (Convert.ToInt32(props[1]), Convert.ToInt32(props[2]));
***REMOVED******REMOVED******REMOVED*** ***REMOVED***
***REMOVED******REMOVED***  ***REMOVED***

***REMOVED******REMOVED***  if (File.Exists(journal.AttachmentsPath))
***REMOVED******REMOVED***  ***REMOVED***
***REMOVED******REMOVED******REMOVED*** var attLines = File.ReadAllLines(journal.AttachmentsPath);
***REMOVED******REMOVED******REMOVED*** foreach (var att in attLines)
***REMOVED******REMOVED******REMOVED*** ***REMOVED***
***REMOVED******REMOVED******REMOVED******REMOVED***var props = att.Split(';');
***REMOVED******REMOVED******REMOVED******REMOVED***journal.ProcessedAttachments[props[0]] = Convert.ToInt32(props[1]);
***REMOVED******REMOVED******REMOVED*** ***REMOVED***
***REMOVED******REMOVED***  ***REMOVED***

***REMOVED******REMOVED***  return journal;
***REMOVED***   ***REMOVED***

***REMOVED***   #endregion

***REMOVED***   private MigrationContext _context;

***REMOVED***   public Dictionary<string, (int, int)> ProcessedRevisions ***REMOVED*** get; private set; ***REMOVED*** = new Dictionary<string, (int, int)>();

***REMOVED***   public Dictionary<string, int> ProcessedAttachments ***REMOVED*** get; private set; ***REMOVED*** = new Dictionary<string, int>();
***REMOVED***   public string ItemsPath ***REMOVED*** get; private set; ***REMOVED***
***REMOVED***   public string AttachmentsPath ***REMOVED*** get; private set; ***REMOVED***

***REMOVED***   public Journal(MigrationContext context)
***REMOVED***   ***REMOVED***
***REMOVED******REMOVED***  _context = context;

***REMOVED******REMOVED***  ItemsPath = Path.Combine(_context.MigrationWorkspace, "itemsJournal.txt");
***REMOVED******REMOVED***  AttachmentsPath = Path.Combine(_context.MigrationWorkspace, "attachmentsJournal.txt");
***REMOVED***   ***REMOVED***

***REMOVED***   public void MarkRevProcessed(string originId, int wiId, int rev)
***REMOVED***   ***REMOVED***

***REMOVED******REMOVED***  ProcessedRevisions[originId] = (wiId, rev);
***REMOVED******REMOVED***  WriteItem(originId, wiId, rev);
***REMOVED***   ***REMOVED***

***REMOVED***   private void WriteItem(string originId, int wiId, int rev)
***REMOVED***   ***REMOVED***
***REMOVED******REMOVED***  File.AppendAllText(ItemsPath, $"***REMOVED***originId***REMOVED***;***REMOVED***wiId***REMOVED***;***REMOVED***rev***REMOVED***" + Environment.NewLine);
***REMOVED***   ***REMOVED***

***REMOVED***   public void MarkAttachmentAsProcessed(string attOriginId, int attWiId)
***REMOVED***   ***REMOVED***
***REMOVED******REMOVED***  ProcessedAttachments.Add(attOriginId, attWiId);
***REMOVED******REMOVED***  WriteAttachment(attOriginId, attWiId);
***REMOVED***   ***REMOVED***

***REMOVED***   private void WriteAttachment(string attOriginId, int attWiId)
***REMOVED***   ***REMOVED***
***REMOVED******REMOVED***  File.AppendAllText(AttachmentsPath, $"***REMOVED***attOriginId***REMOVED***;***REMOVED***attWiId***REMOVED***" + Environment.NewLine);
***REMOVED***   ***REMOVED***

***REMOVED***   public bool IsItemMigrated(string originId, int rev)
***REMOVED***   ***REMOVED***
***REMOVED******REMOVED***  (int, int) migrationResult;
***REMOVED******REMOVED***  if (!ProcessedRevisions.TryGetValue(originId, out migrationResult))
***REMOVED******REMOVED******REMOVED*** return false;

***REMOVED******REMOVED***  (int targetId, int migratedRev) = migrationResult;
***REMOVED******REMOVED***  return rev <= migratedRev;
***REMOVED***   ***REMOVED***

***REMOVED***   public int GetMigratedId(string originId)
***REMOVED***   ***REMOVED***
***REMOVED******REMOVED***  (int, int) migrationResult;
***REMOVED******REMOVED***  if (!ProcessedRevisions.TryGetValue(originId, out migrationResult))
***REMOVED******REMOVED******REMOVED*** return -1;

***REMOVED******REMOVED***  (int wiId, int rev) = migrationResult;

***REMOVED******REMOVED***  return wiId;
***REMOVED***   ***REMOVED***

***REMOVED***   public bool IsAttachmentMigrated(string attOriginId, out int attWiId)
***REMOVED***   ***REMOVED***
***REMOVED******REMOVED***  return ProcessedAttachments.TryGetValue(attOriginId, out attWiId);
***REMOVED***   ***REMOVED***
***REMOVED***
***REMOVED***
