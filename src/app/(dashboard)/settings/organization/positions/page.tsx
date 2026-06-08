import { SimpleMasterDataList } from "@/components/settings/simple-master-data-list";

export default function PositionsPage() {
  return (
    <SimpleMasterDataList
      objectType="Position"
      title="المناصب"
      description="المناصب الوظيفية في المؤسسة"
      backHref="/settings/organization"
      backLabel="إعدادات المؤسسة"
    />
  );
}
