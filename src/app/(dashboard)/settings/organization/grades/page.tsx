import { SimpleMasterDataList } from "@/components/settings/simple-master-data-list";

export default function GradesPage() {
  return (
    <SimpleMasterDataList
      objectType="Grade"
      title="الدرجات الوظيفية"
      description="درجات ومستويات الموظفين"
      backHref="/settings/organization"
      backLabel="إعدادات المؤسسة"
    />
  );
}
