import { SimpleMasterDataList } from "@/components/settings/simple-master-data-list";

export default function CostCentersPage() {
  return (
    <SimpleMasterDataList
      objectType="CostCenter"
      title="مراكز التكلفة"
      description="مراكز التكلفة المحاسبية"
      backHref="/settings/organization"
      backLabel="إعدادات المؤسسة"
    />
  );
}
