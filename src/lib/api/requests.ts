import {
  MasterDataItem, MasterDataItemInput,
  getMasterDataItems, createMasterDataItem, updateMasterDataItem, deleteMasterDataItem, parseMetadata,
} from "./master-data";
import { getLookup, LookupItem } from "./lookups";

// ── Request Types ──
// A Request Type is a MasterDataItem (ObjectType="RequestType"). Identity (code, names,
// icon, colour, status) lives on the item columns; the rich no-code behaviour (category,
// linked form/workflow, SLA, generated document, downstream impact) rides on MetadataJson.

export const REQUEST_TYPE = "RequestType";
export const REQUEST_CATEGORY = "RequestCategory";

export interface RequestTypeMeta {
  categoryId: string;          // RequestCategory master-data id
  formDefinitionId: string;    // linked dynamic form
  workflowDefinitionId: string;
  workflowCode: string;        // stored so submission can start the workflow without a list call
  slaHours: number | null;
  generatesDocument: boolean;
  documentTemplateId: string;
  updatesEmployee: boolean;
  updatesAttendance: boolean;
  updatesPayroll: boolean;
}

export const emptyRequestTypeMeta: RequestTypeMeta = {
  categoryId: "",
  formDefinitionId: "",
  workflowDefinitionId: "",
  workflowCode: "",
  slaHours: null,
  generatesDocument: false,
  documentTemplateId: "",
  updatesEmployee: false,
  updatesAttendance: false,
  updatesPayroll: false,
};

export function getRequestTypeMeta(item: MasterDataItem): RequestTypeMeta {
  return parseMetadata<RequestTypeMeta>(item, emptyRequestTypeMeta);
}

export async function listRequestTypes(includeInactive = true): Promise<MasterDataItem[]> {
  return getMasterDataItems(REQUEST_TYPE, { includeInactive });
}

// Active request types for the employee portal (read-only lookup feed).
export async function getRequestTypeLookup(): Promise<LookupItem[]> {
  return getLookup("request-types");
}

export async function getRequestCategoryLookup(): Promise<LookupItem[]> {
  return getLookup("request-categories");
}

export async function createRequestType(input: MasterDataItemInput): Promise<MasterDataItem> {
  return createMasterDataItem(REQUEST_TYPE, input);
}

export async function updateRequestType(id: string, input: MasterDataItemInput): Promise<MasterDataItem> {
  return updateMasterDataItem(id, input);
}

export async function deleteRequestType(id: string): Promise<void> {
  return deleteMasterDataItem(id);
}
