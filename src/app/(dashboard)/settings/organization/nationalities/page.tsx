import { SimpleMasterDataList } from "@/components/settings/simple-master-data-list";

export default function NationalitiesPage() {
  return (
    <SimpleMasterDataList
      objectType="Nationality"
      title="الجنسيات"
      description="الجنسيات المتاحة في نماذج الموظفين"
      backHref="/settings/organization"
      backLabel="إعدادات المؤسسة"
    />
  );
}
